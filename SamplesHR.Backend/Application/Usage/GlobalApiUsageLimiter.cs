using Raven.Client.Documents;
using Raven.Client.Documents.Operations.TimeSeries;
using SamplesHR.Backend.Application.Exception;

namespace SamplesHR.Backend.Application.Usage;

public class GlobalApiUsageLimiter
{
    private readonly int _maxRequestsPer15Minutes;
    private readonly IDocumentStore _store;

    public GlobalApiUsageLimiter(IDocumentStore store)
    {
        _store = store;

        var raw = Environment.GetEnvironmentVariable(Constants.EnvVars.MaxGlobalRequestsPer15Minutes);
        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException(
                $"Missing \"{Constants.EnvVars.MaxGlobalRequestsPer15Minutes}\" environment variable.");

        if (!int.TryParse(raw, out _maxRequestsPer15Minutes))
            throw new InvalidOperationException(
                $"Invalid integer in \"{Constants.EnvVars.MaxGlobalRequestsPer15Minutes}\": {raw}");
    }

    public async Task EnsureAllowedAsync()
    {
        var result = await _store.Operations.SendAsync(
            new GetTimeSeriesOperation(Constants.DocumentIds.GlobalApiUsage, Constants.TimeSeries.Requests,
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