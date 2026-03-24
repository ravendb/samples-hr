using CommunityToolkit.Aspire.Hosting.RavenDB;

var builder = DistributedApplication.CreateBuilder(args);

var license = Environment.GetEnvironmentVariable("SAMPLES_HR_RAVEN_LICENSE");

var settings = RavenDBServerSettings.Unsecured();
if (license != null)
{
    // Use the license from the environmental variable
    settings.WithLicense(license);
}

// High enough so that they don't collide with other local things run on 8080 etc
settings.Port = 9349;
settings.TcpPort = 41349;

var ravenServer = builder
    .AddRavenDB("ravendb", settings)
    .WithImage("ravendb/ravendb-nightly", "7.2.2-nightly-20260324-0722-ubuntu.22.04-x64")
    .WithIconName("Database");

const string dbName = "HRAssistant";

var ravenDatabase = ravenServer.AddDatabase(dbName);

var frontend = builder.AddNpmApp("frontend", "../sampleshr-frontend")
    .WithHttpEndpoint(env: "PORT")
    .PublishAsDockerFile();

var backend = builder.AddProject<Projects.SamplesHR_Backend>("backend")
    .WithReference(ravenDatabase)
    .WithReference(frontend)
    .WaitFor(ravenDatabase)
    .WithEnvironment("SAMPLES_HR_OPENAI_API_KEY", Environment.GetEnvironmentVariable("SAMPLES_HR_OPENAI_API_KEY"))
    .WithHttpCommand(
    path: "/api/seed/all",
    displayName: "Seed data",
    endpointName: "http",
    commandOptions: new HttpCommandOptions
    {
        Description = "Seed the database with sample data",
        IconName = "databaseArrowUp",
        IsHighlighted = true
    });

frontend
    .WithReference(backend)
    .WaitFor(backend)
    .WithEnvironment("REACT_APP_BACKEND_URL", backend.GetEndpoint("http"));

builder.Build().Run();
