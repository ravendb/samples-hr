using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SamplesHR.Seeder;

internal sealed class SeedAndExitWorker(
    SampleDataSeeder seeder,
    IHostApplicationLifetime lifetime,
    ILogger<SeedAndExitWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await seeder.SeedAllAsync(stoppingToken);
            logger.LogInformation("Seed complete.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Seed failed.");
            Environment.Exit(1);
            return;
        }
        lifetime.StopApplication();
    }
}
