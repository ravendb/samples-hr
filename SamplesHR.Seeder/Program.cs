using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SamplesHR.Seeder;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.AddRavenDBClient("HRAssistant", configureSettings: settings => settings.CreateDatabase = true);
builder.Services.AddSingleton<SampleDataSeeder>();
builder.Services.AddHostedService<SeedAndExitWorker>();

await builder.Build().RunAsync();
