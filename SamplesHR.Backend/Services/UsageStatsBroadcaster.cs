using Microsoft.AspNetCore.SignalR;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations.TimeSeries;
using SamplesHR.Backend.Application.Usage;
using SamplesHR.Backend.Hubs;

namespace SamplesHR.Backend.Services;

public class UsageStatsBroadcaster : BackgroundService
{
    private readonly IHubContext<UsageStatsHub> _hubContext;
    private readonly IDocumentStore _store;
    private readonly int _maxGlobalRequestsPer15Minutes;
    private readonly int _maxSessionRequestsPer30Seconds;
    private readonly ILogger<UsageStatsBroadcaster> _logger;

    public UsageStatsBroadcaster(
        IHubContext<UsageStatsHub> hubContext,
        IDocumentStore store,
        SessionApiUsageLimiter sessionLimiter,
        ILogger<UsageStatsBroadcaster> logger)
    {
        _hubContext = hubContext;
        _store = store;
        _logger = logger;
        _maxSessionRequestsPer30Seconds = sessionLimiter.MaxRequestsPer30Seconds;

        var raw = Environment.GetEnvironmentVariable(Constants.EnvVars.MaxGlobalRequestsPer15Minutes);
        if (string.IsNullOrWhiteSpace(raw) || !int.TryParse(raw, out _maxGlobalRequestsPer15Minutes))
        {
            _maxGlobalRequestsPer15Minutes = 100; // fallback default
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UsageStatsBroadcaster started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Broadcast global stats to all clients
                var globalStats = await GetGlobalStatsAsync();
                await _hubContext.Clients.All.SendAsync("GlobalUsageUpdate", globalStats, stoppingToken);

                // Broadcast session stats to each active session group
                foreach (var sessionId in UsageStatsHub.GetActiveSessionIds())
                {
                    try
                    {
                        var sessionStats = await GetSessionStatsAsync(sessionId);
                        await _hubContext.Clients.Group($"session:{sessionId}")
                            .SendAsync("SessionUsageUpdate", sessionStats, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error broadcasting session stats for {SessionId}", sessionId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting usage stats");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task<UsageUpdate> GetGlobalStatsAsync()
    {
        var now = DateTime.UtcNow;
        var windowStart = now.AddMinutes(-15);

        var result = await _store.Operations.SendAsync(
            new GetTimeSeriesOperation(Constants.DocumentIds.GlobalApiUsage, Constants.TimeSeries.Requests, windowStart, now));

        var entries = result?.Entries ?? [];

        // Send individual request timestamps with session ID from tag
        var points = entries
            .Select(e => new DataPoint(e.Timestamp, 1, e.Tag))
            .OrderBy(p => p.Timestamp)
            .ToList();

        return new UsageUpdate(
            CurrentUsage: entries.Length,
            MaxRequests: _maxGlobalRequestsPer15Minutes,
            RequestsLeft: Math.Max(0, _maxGlobalRequestsPer15Minutes - entries.Length),
            Timestamp: now,
            WindowStart: windowStart,
            RecentPoints: points
        );
    }

    public async Task<UsageUpdate> GetSessionStatsAsync(string sessionId)
    {
        var now = DateTime.UtcNow;
        var windowStart = now.AddSeconds(-30);

        var result = await _store.Operations.SendAsync(
            new GetTimeSeriesOperation(Constants.DocumentIds.SessionApiUsage(sessionId), Constants.TimeSeries.Requests, windowStart, now));

        var entries = result?.Entries ?? [];

        // Send individual request timestamps (each entry is one request)
        var points = entries
            .Select(e => new DataPoint(e.Timestamp, 1))
            .OrderBy(p => p.Timestamp)
            .ToList();

        return new UsageUpdate(
            CurrentUsage: entries.Length,
            MaxRequests: _maxSessionRequestsPer30Seconds,
            RequestsLeft: Math.Max(0, _maxSessionRequestsPer30Seconds - entries.Length),
            Timestamp: now,
            WindowStart: windowStart,
            RecentPoints: points
        );
    }
}

