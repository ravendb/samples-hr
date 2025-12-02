using Raven.Client.Documents;
using Raven.Client.Documents.Operations.AI;

namespace SamplesHR.Backend.Application.Usage;

public class GlobalApiUsageTracker
{
    private readonly IDocumentStore _store;

    public GlobalApiUsageTracker(IDocumentStore store)
    {
        _store = store;
    }

    public async Task TrackGlobalAsync(HttpContext http, AiUsage usage)
    {
        var sessionId = (string?)http.Items["SessionId"]
                        ?? throw new System.Exception("Missing SessionId in HttpContext.");

        using var session = _store.OpenAsyncSession();

        var id = Constants.DocumentIds.GlobalApiUsage;

        var doc = await session.LoadAsync<GlobalApiUsage>(id);
        if (doc == null)
        {
            doc = new GlobalApiUsage();
            await session.StoreAsync(doc);
        }

        // Append TS with session ID as tag
        session.TimeSeriesFor(doc, Constants.TimeSeries.Requests)
            .Append(DateTime.UtcNow, 1, sessionId);

        session.CountersFor(doc)
            .Increment("TotalCompletionTokens", usage.CompletionTokens);

        session.CountersFor(doc)
            .Increment("TotalPromptTokens", usage.PromptTokens);

        session.CountersFor(doc)
            .Increment("TotalCachedTokens", usage.CachedTokens);


        await session.SaveChangesAsync();
    }
}