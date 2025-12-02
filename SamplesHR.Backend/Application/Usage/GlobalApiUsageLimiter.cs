using Raven.Client.Documents;
using Raven.Client.Documents.Operations.TimeSeries;
using SamplesHR.Backend.Application.Exception;

namespace SamplesHR.Backend.Application.Usage;

public class GlobalApiUsageLimiter
{
    private readonly int _maxRequestsPer15Minutes;
    private readonly IDocumentStore _store;

    private const string MaxRequestsPer15MinEnvVar = 
        "SAMPLES_HR_MAX_GLOBAL_REQUESTS_PER_15_MINUTES";

    public GlobalApiUsageLimiter(IDocumentStore store)
    {
        _store = store;

        var raw = Environment.GetEnvironmentVariable(MaxRequestsPer15MinEnvVar);
        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException(
                $"Missing \"{MaxRequestsPer15MinEnvVar}\" environment variable.");

        if (!int.TryParse(raw, out _maxRequestsPer15Minutes))
            throw new InvalidOperationException(
                $"Invalid integer in \"{MaxRequestsPer15MinEnvVar}\": {raw}");
    }

    public async Task EnsureAllowedAsync()
    {
        var id = "GlobalApiUsageLimiter/global";

        var result = await _store.Operations.SendAsync(
            new GetTimeSeriesOperation(id, "Requests",
                DateTime.UtcNow.AddMinutes(-15),
                DateTime.UtcNow));

        var entries = result?.Entries;  //TODO: change to rollup/aggregation?

        if (entries == null || entries.Length == 0) 
            return;

        int count = entries.Length;

        if (count >= _maxRequestsPer15Minutes)
        {
            throw new GlobalRateLimitExceededException(
                "Demo request limit exceeded. Service is too busy right now. Please try again in a short while."
            );
        }
    }
}