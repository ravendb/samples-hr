using CommunityToolkit.Aspire.Hosting.RavenDB;

var builder = DistributedApplication.CreateBuilder(args);

// Parameters
var ravenDbLicense = builder
    .AddParameter("ravendb-license", secret: true)
    .WithDescription("Your Developer license formatted as JSON.");

var openAiApiKey = builder.AddParameter("openai-api-key", secret: true)
    .WithDescription("OpenAI API key");

var maxGlobalRequests = builder.AddParameter("max-global-requests-per-15-min", "100")
    .WithDescription("Maximum API requests globally per 15 minutes");

var maxSessionRequests = builder.AddParameter("max-session-requests-per-30-sec", "5")
    .WithDescription("Maximum API requests per session per 30 seconds");

var settings = RavenDBServerSettings.Unsecured();

// High enough so that they don't collide with other local things run on 8080 etc
settings.Port = 9349;
settings.TcpPort = 41349;

var ravenServer = builder
    .AddRavenDB("ravendb", settings)
    .WithImage("ravendb/ravendb", "7.2-latest")
    .WithIconName("Database")
    .WithEnvironment("RAVEN_License_Eula_Accepted", "true")
    .WithEnvironment("RAVEN_License", ravenDbLicense);

const string dbName = "HRAssistant";

var ravenDatabase = ravenServer.AddDatabase(dbName);

var seeder = builder.AddProject<Projects.SamplesHR_Seeder>("seeder")
    .WithReference(ravenDatabase)
    .WaitFor(ravenDatabase)
    .WithIconName("DatabaseArrowUp");

var frontend = builder.AddNpmApp("frontend", "../sampleshr-frontend")
    .WithHttpEndpoint(env: "PORT")
    .WithEnvironment("BROWSER", "none")
    .PublishAsDockerFile();

var backend = builder.AddProject<Projects.SamplesHR_Backend>("backend")
    .WithReference(ravenDatabase)
    .WithReference(frontend)
    .WaitFor(ravenDatabase)
    .WaitForCompletion(seeder)
    .WithEnvironment("SAMPLES_HR_OPENAI_API_KEY", openAiApiKey)
    .WithEnvironment("SAMPLES_HR_MAX_GLOBAL_REQUESTS_PER_15_MINUTES", maxGlobalRequests)
    .WithEnvironment("SAMPLES_HR_MAX_SESSION_REQUESTS_PER_30_SECONDS", maxSessionRequests);

frontend
    .WithReference(backend)
    .WaitFor(backend)
    .WithEnvironment("REACT_APP_BACKEND_URL", backend.GetEndpoint("http"));

builder.Build().Run();
