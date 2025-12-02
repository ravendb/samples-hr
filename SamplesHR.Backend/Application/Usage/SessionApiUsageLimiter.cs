using Raven.Client.Documents;
using Raven.Client.Documents.Operations.TimeSeries;
using SamplesHR.Backend.Application.Exception;

namespace SamplesHR.Backend.Application.Usage;

public class SessionApiUsageLimiter
{
    private readonly int _maxRequestsPer30Seconds;
    private readonly IDocumentStore _store;

    public int MaxRequestsPer30Seconds => _maxRequestsPer30Seconds;

    public SessionApiUsageLimiter(IDocumentStore store)
    {
        _store = store;

        var raw = Environment.GetEnvironmentVariable(Constants.EnvVars.MaxSessionRequestsPer30Seconds);
        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException(
                $"Missing \"{Constants.EnvVars.MaxSessionRequestsPer30Seconds}\" environment variable.");

        if (!int.TryParse(raw, out _maxRequestsPer30Seconds))
            throw new InvalidOperationException(
                $"Invalid integer in \"{Constants.EnvVars.MaxSessionRequestsPer30Seconds}\": {raw}");
    }

    public async Task EnsureAllowedAsync(string sessionId)
    {
        var result = await _store.Operations.SendAsync(
            new GetTimeSeriesOperation(Constants.DocumentIds.SessionApiUsage(sessionId), Constants.TimeSeries.Requests,
                DateTime.UtcNow.AddSeconds(-30),
                DateTime.UtcNow));

        var entries = result?.Entries;

        if (entries == null || entries.Length == 0)
            return;

        int count = entries.Length;

        if (count >= _maxRequestsPer30Seconds)
        {
            throw new GlobalRateLimitExceededException(
                "Session request limit exceeded. You're typing too fast! :) Try again in a short while."
            );
        }
    }
}

