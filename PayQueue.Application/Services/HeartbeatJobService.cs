using PayQueue.Application.Jobs;
using PayQueue.Domain.Scheduling;
using PayQueue.Domain.Services;

namespace PayQueue.Application.Services;

public sealed class HeartbeatJobService(IJobScheduler jobScheduler) : IHeartbeatJobService
{
    private const string RecurringJobId = "sample-heartbeat-recurring";

    public Task EnqueueAsync(CancellationToken cancellationToken)
    {
        jobScheduler.Enqueue<SampleHeartbeatJob>(job => job.ExecuteAsync(default));
        return Task.CompletedTask;
    }

    public Task ScheduleRecurringAsync(CancellationToken cancellationToken)
    {
        jobScheduler.ScheduleRecurring<SampleHeartbeatJob>(
            RecurringJobId,
            job => job.ExecuteAsync(default),
            CronExpressions.EveryFiveMinutes);

        return Task.CompletedTask;
    }
}


