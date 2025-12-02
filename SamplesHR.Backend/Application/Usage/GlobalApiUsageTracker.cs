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

    public async Task TrackGlobalAsync(AiUsage usage)
    {
        using var session = _store.OpenAsyncSession();

        var id = "GlobalApiUsageLimiter/global";

        var doc = await session.LoadAsync<GlobalApiUsage>(id);
        if (doc == null)
        {
            doc = new GlobalApiUsage();
            await session.StoreAsync(doc);
        }

        // Append TS
        session.TimeSeriesFor(doc, "Requests")
            .Append(DateTime.UtcNow, 1);

        session.CountersFor(doc)
            .Increment("TotalCompletionTokens", usage.CompletionTokens);
        
        session.CountersFor(doc)
            .Increment("TotalPromptTokens", usage.PromptTokens);
        
        session.CountersFor(doc)
            .Increment("TotalCachedTokens", usage.CachedTokens);


        await session.SaveChangesAsync();
    }
}