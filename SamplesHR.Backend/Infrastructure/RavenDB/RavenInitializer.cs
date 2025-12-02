using Raven.Client.Documents;
using Raven.Client.Documents.Operations.AI;
using Raven.Client.Documents.Operations.ConnectionStrings;
using Raven.Client.Documents.Operations.Expiration;
using Raven.Client.Documents.Operations.TimeSeries;

namespace SamplesHR.Backend.Infrastructure.RavenDB;

public class RavenInitializer(IDocumentStore store) : IHostedService
{
    private static readonly TimeSeriesConfiguration TimeSeriesConfig = new()
    {
        Collections =
        {
            { "ApiUsageSession", new TimeSeriesCollectionConfiguration() },
            { "GlobalApiUsageLimiter", new TimeSeriesCollectionConfiguration() }
        }
    };
    
    private static readonly ExpirationConfiguration ExpirationConfig = new()
    {
        Disabled = false,
        DeleteFrequencyInSec = 60
    };

    private static readonly AiConnectionString AgentOpenAiConnectionString = new()
    {
        Name = "Human Resources' AI Model",
        ModelType = AiModelType.Chat,
        OpenAiSettings = new OpenAiSettings
        {
            ApiKey = Environment.GetEnvironmentVariable(Constants.EnvVars.OpenAiApiKey),
            Model = "gpt-5-mini",
            Endpoint = "https://api.openai.com/v1",
        }
    };

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // 1) EXPIRATION
        await store.Maintenance.SendAsync(
            new ConfigureExpirationOperation(ExpirationConfig), cancellationToken);

        // 2) TIME SERIES
        await store.Maintenance.SendAsync(new ConfigureTimeSeriesOperation(TimeSeriesConfig), cancellationToken);
        
        // 3) AI CONNECTION STRING
        await store.Maintenance.SendAsync(
            new PutConnectionStringOperation<AiConnectionString>(AgentOpenAiConnectionString), cancellationToken);
        
        // 3) AI AGENT
        await HumanResourcesAgentCreator.Create(store);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}