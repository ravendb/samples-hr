using SamplesHR.Backend.Application.Usage;
using SamplesHR.Backend.Hubs;
using SamplesHR.Backend.Infrastructure.Middleware;
using SamplesHR.Backend.Infrastructure.RavenDB;
using SamplesHR.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

const string frontendHttpUrlConfigurationKey = "services:frontend:http:0";
const string chatApiPath = "/api/humanresourcesagent/chat";

// Add Aspire Service Defaults (telemetry, health checks, service discovery)
builder.AddServiceDefaults();

builder.AddRavenDBClient("HRAssistant", configureSettings: settings => settings.CreateDatabase = true);

builder.Services.AddHostedService<RavenInitializer>();

var frontendUrl = builder.Configuration[frontendHttpUrlConfigurationKey] ??
                  throw new NullReferenceException("Frontend URL is not configured by Aspire properly.");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add SignalR
builder.Services.AddSignalR();

// Add ASP.NET controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddSingleton<SessionApiUsageLimiter>();
builder.Services.AddSingleton<GlobalApiUsageLimiter>();

builder.Services.AddSingleton<SessionApiUsageTracker>();
builder.Services.AddSingleton<GlobalApiUsageTracker>();

builder.Services.AddSingleton<UsageStatsBroadcaster>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<UsageStatsBroadcaster>());

builder.Services.AddHttpContextAccessor(); // needed by UsageLimiter

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

app.UseWhen(
    ctx => ctx.Request.Path.Equals(chatApiPath, StringComparison.OrdinalIgnoreCase),
    branch =>
    {
        branch.UseMiddleware<ChatEphemeralSessionMiddleware>();
    });

app.MapControllers();

app.MapHub<UsageStatsHub>("/hubs/usage");

app.Run();
