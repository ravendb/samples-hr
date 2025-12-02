var builder = DistributedApplication.CreateBuilder(args);

var ravenServer = builder.AddRavenDB("ravendb")
    .WithImage("ravendb/ravendb", "7.1-latest")
    .WithIconName("Database")
    .WithEnvironment("RAVEN_LICENSE", Environment.GetEnvironmentVariable("SAMPLES_HR_RAVEN_LICENSE"));

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
    commandOptions: new HttpCommandOptions()
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
