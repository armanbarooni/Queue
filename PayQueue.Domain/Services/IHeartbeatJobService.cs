namespace PayQueue.Domain.Services;

public interface IHeartbeatJobService
{
    Task EnqueueAsync(CancellationToken cancellationToken);

    Task ScheduleRecurringAsync(CancellationToken cancellationToken);
}


