using Microsoft.Extensions.DependencyInjection;
using PayQueue.Application.Jobs;
using PayQueue.Domain.Scheduling;
using PayQueue.Domain.Services;

namespace PayQueue.Worker;

public sealed class Worker(
    ILogger<Worker> logger,
    IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("PayQueue.Worker starting up.");

        using var scope = scopeFactory.CreateScope();
        var heartbeatJobService = scope.ServiceProvider.GetRequiredService<IHeartbeatJobService>();
        var jobScheduler = scope.ServiceProvider.GetRequiredService<IJobScheduler>();

        await heartbeatJobService.ScheduleRecurringAsync(cancellationToken);

        jobScheduler.ScheduleRecurring<RefundInquiryJob>(
            "refund-inquiry-recurring",
            job => job.ExecuteAsync(default),
            CronExpressions.EveryFiveMinutes);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("PayQueue.Worker is stopping.");
        return Task.CompletedTask;
    }
}

