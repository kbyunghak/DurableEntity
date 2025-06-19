using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("host.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

// Azure Functions
builder.ConfigureFunctionsWebApplication();

// Durable Entities
builder.Services.AddDurableTaskClient(options => options.UseGrpc());
builder.Services.AddSingleton<ConnectorStateEntity>();

// Logging
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddConsole();
});

builder.Build().Run();
