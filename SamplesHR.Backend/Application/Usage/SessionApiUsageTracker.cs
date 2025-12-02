using Raven.Client.Documents;
using Raven.Client.Documents.Operations.AI;

namespace SamplesHR.Backend.Application.Usage;

public class SessionApiUsageTracker(IDocumentStore store)
{
    public async Task<SessionApiUsage> TrackAsync(HttpContext http, AiUsage usage)
    {
        var sessionId = (string?)http.Items["SessionId"]
                        ?? throw new System.Exception("Missing SessionId in HttpContext.");

        using var session = store.OpenAsyncSession();

        var id = Constants.DocumentIds.SessionApiUsage(sessionId);
        var doc = await session.LoadAsync<SessionApiUsage>(id);

        if (doc == null)
        {
            doc = new SessionApiUsage
            {
                Id = id,
                SessionId = sessionId,
                CreatedAt = DateTime.UtcNow,
                UserAgent = http.Items["UserAgent"] as string,
                ClientIP = http.Items["ClientIP"] as string,
                AcceptLanguage = http.Items["AcceptLanguage"] as string,
                Referer = http.Items["Referer"] as string,
                UTM = new UtmInfo
                {
                    Source = http.Items["UtmSource"] as string,
                    Medium = http.Items["UtmMedium"] as string,
                    Campaign = http.Items["UtmCampaign"] as string,
                    Term = http.Items["UtmTerm"] as string,
                    Content = http.Items["UtmContent"] as string
                }
            };

            await session.StoreAsync(doc);
        }

        session.TimeSeriesFor(doc, Constants.TimeSeries.Requests)
            .Append(DateTime.UtcNow, 1);

        session.CountersFor(doc)
            .Increment("TotalCompletionTokens", usage.CompletionTokens);

        session.CountersFor(doc)
            .Increment("TotalPromptTokens", usage.PromptTokens);

        session.CountersFor(doc)
            .Increment("TotalCachedTokens", usage.CachedTokens);

        await session.SaveChangesAsync();

        return doc;
    }
}