using System.IO;
using Hangfire;
using HealthChecks.UI.Client;
using PayQueue.Application;
using PayQueue.Application.Jobs;
using PayQueue.Domain.Services;
using PayQueue.Infrastructure.Monitoring;
using PayQueue.Infrastructure.Refunds;
using PayQueue.Infrastructure.Scheduling;
using PayQueue.Shared.Configuration;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    var contentRoot = context.HostingEnvironment.ContentRootPath;
    var projectRoot = Path.GetFullPath(Path.Combine(contentRoot, ".."));
    var logDirectory = Path.Combine(projectRoot, "logs");
    Directory.CreateDirectory(logDirectory);
    var logPath = Path.Combine(logDirectory, "app.log");

    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("ContentRootPath", contentRoot)
        .WriteTo.File(
            logPath,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}");
});

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddApplicationServices();
builder.Services
    .AddHangfireInfrastructure(builder.Configuration)
    .AddRefundInfrastructure(builder.Configuration)
    .AddMonitoringInfrastructure(builder.Configuration);

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("PayQueue.Api"))
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddHttpClientInstrumentation();
        metrics.AddPrometheusExporter();
    })
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();
    });

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var hangfireOptions = app.Services.GetRequiredService<IOptions<HangfireOptions>>().Value;
if (hangfireOptions.EnableDashboard)
{
    app.UseHangfireDashboard(hangfireOptions.DashboardPath);
}

var jobApi = app.MapGroup("/api/jobs");

jobApi.MapPost("/sample-heartbeat", async (IHeartbeatJobService heartbeatJobService, CancellationToken cancellationToken) =>
    {
        await heartbeatJobService.EnqueueAsync(cancellationToken);
        return Results.Accepted();
    })
    .WithName("enqueue-sample-heartbeat")
    .WithSummary("Enqueue a sample heartbeat job");

jobApi.MapPost("/sample-heartbeat/recurring", async (IHeartbeatJobService heartbeatJobService, CancellationToken cancellationToken) =>
    {
        await heartbeatJobService.ScheduleRecurringAsync(cancellationToken);
        return Results.Ok();
    })
    .WithName("schedule-sample-heartbeat")
    .WithSummary("Register a recurring sample heartbeat job that runs every 5 minutes");

jobApi.MapPost("/refunds/inquiry", (IBackgroundJobClient backgroundJobs) =>
    {
        backgroundJobs.Enqueue<RefundInquiryJob>(job => job.ExecuteAsync(CancellationToken.None));
        return Results.Accepted();
    })
    .WithName("enqueue-refund-inquiry")
    .WithSummary("Trigger an immediate refund inquiry job");

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapGet("/", () => hangfireOptions.EnableDashboard
        ? Results.Redirect(hangfireOptions.DashboardPath)
        : Results.Ok("PayQueue service is running."))
    .ExcludeFromDescription();

app.MapPrometheusScrapingEndpoint();

app.Run();



