using Raven.Client.Documents;
using Raven.Client.Documents.Operations.TimeSeries;
using SamplesHR.Backend.Application.Exception;

namespace SamplesHR.Backend.Application.Usage;

public class SessionApiUsageLimiter
{
    private readonly int _maxRequestsPer30Seconds;
    private readonly IDocumentStore _store;

    private const string MaxRequestsPer30SecEnvVar = 
        "SAMPLES_HR_MAX_SESSION_REQUESTS_PER_30_SECONDS";

    public SessionApiUsageLimiter(IDocumentStore store)
    {
        _store = store;
        
        var raw = Environment.GetEnvironmentVariable(MaxRequestsPer30SecEnvVar);
        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException(
                $"Missing \"{MaxRequestsPer30SecEnvVar}\" environment variable.");

        if (!int.TryParse(raw, out _maxRequestsPer30Seconds))
            throw new InvalidOperationException(
                $"Invalid integer in \"{MaxRequestsPer30SecEnvVar}\": {raw}");
    }

    public async Task EnsureAllowedAsync(string sessionId)
    {
        var id = $"ApiUsageSession/{sessionId}";

        var result = await _store.Operations.SendAsync(
            new GetTimeSeriesOperation(id, "Requests",
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

