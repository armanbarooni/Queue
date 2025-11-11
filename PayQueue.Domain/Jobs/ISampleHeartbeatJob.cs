namespace PayQueue.Domain.Jobs;

public interface ISampleHeartbeatJob
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}


