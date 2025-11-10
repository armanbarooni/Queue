using TandemQueue.Application;
using TandemQueue.Infrastructure.Refunds;
using TandemQueue.Infrastructure.Scheduling;
using TandemQueue.Worker;
using Serilog;
using Serilog.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddProvider(new SerilogLoggerProvider(Log.Logger, dispose: false));

builder.Services.AddApplicationServices();
builder.Services
    .AddHangfireInfrastructure(builder.Configuration)
    .AddRefundInfrastructure(builder.Configuration)
    .AddConfiguredHangfireServer();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

try
{
    host.Run();
}
finally
{
    Log.CloseAndFlush();
}

