import React, { useEffect, useState, useRef, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';
import './UsageCharts.css';

interface DataPoint {
  timestamp: string;
  count: number;
  sessionId?: string | null;
}

interface UsageUpdate {
  currentUsage: number;
  maxRequests: number;
  requestsLeft: number;
  timestamp: string;
  windowStart: string;
  recentPoints: DataPoint[];
}

export interface RateLimitAlert {
  message: string;
  isSession: boolean;
}

const API_BASE_URL = process.env.REACT_APP_BACKEND_URL;

interface SingleChartProps {
  title: string;
  stats: UsageUpdate | null;
  color: string;
  windowLabel: string;
  currentSessionId?: string | null;
  ownColor?: string;
}

const SingleChart: React.FC<SingleChartProps> = ({ title, stats, color, windowLabel, currentSessionId, ownColor }) => {
  const [hoveredDot, setHoveredDot] = useState<{ x: number; timestamp: string } | null>(null);

  const usagePercentage = stats ? (stats.currentUsage / stats.maxRequests) * 100 : 0;
  const isWarning = usagePercentage >= 75;
  const isCritical = usagePercentage >= 90;

  const getDotPositions = () => {
    if (!stats || !stats.recentPoints.length) return [];
    const windowStart = new Date(stats.windowStart).getTime();
    const windowEnd = new Date(stats.timestamp).getTime();
    const windowDuration = windowEnd - windowStart;

    return stats.recentPoints.map((point) => {
      const pointTime = new Date(point.timestamp).getTime();
      const position = ((pointTime - windowStart) / windowDuration) * 100;
      const isOwnRequest = currentSessionId && point.sessionId === currentSessionId;
      return {
        position: Math.max(0, Math.min(100, position)),
        timestamp: point.timestamp,
        isOwn: isOwnRequest,
      };
    });
  };

  const formatTime = (timestamp: string) => {
    const date = new Date(timestamp);
    return date.toLocaleTimeString([], {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      fractionalSecondDigits: 2
    } as Intl.DateTimeFormatOptions);
  };

  const dotPositions = getDotPositions();

  return (
    <div className="single-chart">
      <div className="chart-header">
        <span className="chart-title">{title}</span>
        <div className={`chart-stats ${isCritical ? 'critical' : isWarning ? 'warning' : ''}`}>
          <span className="current" style={{ color: isCritical ? '#ef4444' : isWarning ? '#eab308' : color }}>
            {stats?.currentUsage ?? 0}
          </span>
          <span className="separator">/</span>
          <span className="max">{stats?.maxRequests ?? 0}</span>
        </div>
      </div>

      <div className="chart-bar-container">
        <div
          className={`chart-bar ${isCritical ? 'critical' : isWarning ? 'warning' : ''}`}
          style={{
            width: `${Math.min(usagePercentage, 100)}%`,
            background: isCritical
              ? 'linear-gradient(90deg, #ef4444 0%, #dc2626 100%)'
              : isWarning
                ? 'linear-gradient(90deg, #eab308 0%, #f59e0b 100%)'
                : `linear-gradient(90deg, ${color} 0%, ${color}cc 100%)`
          }}
        />
      </div>

      <div className="timeline-container">
        <div className="timeline-track">
          {dotPositions.map((dot, index) => {
            const dotColor = dot.isOwn && ownColor ? ownColor : color;
            return (
              <div
                key={index}
                className="timeline-dot-wrapper"
                style={{ left: `${dot.position}%` }}
                onMouseEnter={() => setHoveredDot({ x: dot.position, timestamp: dot.timestamp })}
                onMouseLeave={() => setHoveredDot(null)}
              >
                <div
                  className="timeline-dot"
                  style={{
                    backgroundColor: dotColor,
                    boxShadow: `0 0 6px ${dotColor}80`
                  }}
                />
                {hoveredDot?.timestamp === dot.timestamp && (
                  <div className="timeline-tooltip">
                    {formatTime(dot.timestamp)}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </div>

      <div className="chart-footer">
        <span className="window-label">{windowLabel}</span>
        <span className="requests-left">{stats?.requestsLeft ?? 0} left</span>
      </div>
    </div>
  );
};

interface UsageChartsProps {
  rateLimitAlert?: RateLimitAlert | null;
  onAlertDismiss?: () => void;
}

export const UsageCharts: React.FC<UsageChartsProps> = ({ rateLimitAlert, onAlertDismiss }) => {
  const [globalStats, setGlobalStats] = useState<UsageUpdate | null>(null);
  const [sessionStats, setSessionStats] = useState<UsageUpdate | null>(null);
  const [connectionState, setConnectionState] = useState<'connecting' | 'connected' | 'disconnected'>('connecting');
  const [currentSessionId, setCurrentSessionId] = useState<string | null>(null);
  const [isExpanded, setIsExpanded] = useState(false);
  const [isBlinking, setIsBlinking] = useState(false);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const sessionIdRef = useRef<string | null>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);

  // Handle rate limit alert - blink and expand
  useEffect(() => {
    if (rateLimitAlert) {
      setIsBlinking(true);
      setIsExpanded(true);
      const timer = setTimeout(() => setIsBlinking(false), 3000);
      return () => clearTimeout(timer);
    }
  }, [rateLimitAlert]);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsExpanded(false);
        if (rateLimitAlert && onAlertDismiss) {
          onAlertDismiss();
        }
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [rateLimitAlert, onAlertDismiss]);

  const toggleExpanded = useCallback(() => {
    setIsExpanded(prev => {
      if (prev && rateLimitAlert && onAlertDismiss) {
        onAlertDismiss();
      }
      return !prev;
    });
  }, [rateLimitAlert, onAlertDismiss]);

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/usage`, {
        withCredentials: true,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connectionRef.current = connection;

    connection.on('GlobalUsageUpdate', (update: UsageUpdate) => {
      setGlobalStats(update);
    });

    connection.on('SessionUsageUpdate', (update: UsageUpdate) => {
      setSessionStats(update);
    });

    connection.on('SessionIdReceived', (sessionId: string) => {
      if (sessionId) {
        sessionIdRef.current = sessionId;
        setCurrentSessionId(sessionId);
      }
    });

    connection.onreconnecting(() => setConnectionState('connecting'));
    connection.onreconnected(() => setConnectionState('connected'));
    connection.onclose(() => setConnectionState('disconnected'));

    connection
      .start()
      .then(() => setConnectionState('connected'))
      .catch((err) => {
        console.error('SignalR connection error:', err);
        setConnectionState('disconnected');
      });

    return () => {
      connection.stop();
    };
  }, []);

  return (
    <div className={`usage-dropdown ${isBlinking ? 'blinking' : ''}`} ref={dropdownRef}>
      <button className="usage-dropdown-toggle" onClick={toggleExpanded}>
        <span className="toggle-title">API Usage</span>
        <span className={`connection-indicator ${connectionState}`}>
          {connectionState === 'connected' ? '●' : '○'}
        </span>
        <span className={`toggle-arrow ${isExpanded ? 'expanded' : ''}`}>▼</span>
      </button>

      {isExpanded && (
        <div className="usage-dropdown-content">
          {rateLimitAlert && (
            <div className="rate-limit-alert">
              <div className="alert-message">
                {rateLimitAlert.isSession ? '⚠️' : '🚨'} {rateLimitAlert.message}
              </div>
              <div className="alert-info">
                We're tracking your API usage with RavenDB Time Series - learn how they work{' '}
                <a
                  href="https://ravendb.net/features#:~:text=dimensional%20vector%20embeddings-,TIME%20SERIES,-Distributed%20Time%20Series"
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  here
                </a>
                .
              </div>
            </div>
          )}

          <div className="charts-grid">
            <SingleChart
              title="YOUR SESSION"
              stats={sessionStats}
              color="#22c55e"
              windowLabel="30 sec window"
            />
            <SingleChart
              title="GLOBAL"
              stats={globalStats}
              color="#6366f1"
              windowLabel="15 min window"
              currentSessionId={currentSessionId}
              ownColor="#22c55e"
            />
          </div>
        </div>
      )}
    </div>
  );
};

