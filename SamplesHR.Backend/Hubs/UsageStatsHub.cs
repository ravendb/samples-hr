using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using SamplesHR.Backend.Services;

namespace SamplesHR.Backend.Hubs;

public class UsageStatsHub : Hub
{
    private readonly UsageStatsBroadcaster _broadcaster;

    // Track active session subscriptions (sessionId -> connection count)
    private static readonly ConcurrentDictionary<string, int> ActiveSessions = new();

    public UsageStatsHub(UsageStatsBroadcaster broadcaster)
    {
        _broadcaster = broadcaster;
    }

    public override async Task OnConnectedAsync()
    {
        var sessionId = Context.GetHttpContext()?.Request.Cookies[Constants.Cookies.SessionId];
        if (!string.IsNullOrEmpty(sessionId))
        {
            // Auto-subscribe to session group and track
            await Groups.AddToGroupAsync(Context.ConnectionId, $"session:{sessionId}");
            ActiveSessions.AddOrUpdate(sessionId, 1, (_, count) => count + 1);
            Context.Items["SessionId"] = sessionId;

            // Send session ID and initial stats to client
            await Clients.Caller.SendAsync("SessionIdReceived", sessionId);
            var stats = await _broadcaster.GetSessionStatsAsync(sessionId);
            await Clients.Caller.SendAsync("SessionUsageUpdate", stats);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items["SessionId"] is string sessionId)
        {
            ActiveSessions.AddOrUpdate(sessionId, 0, (_, count) => count - 1);
            if (ActiveSessions.TryGetValue(sessionId, out var count) && count <= 0)
            {
                ActiveSessions.TryRemove(sessionId, out _);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public static IEnumerable<string> GetActiveSessionIds() => ActiveSessions.Keys;
}

