using PayQueue.Domain.Jobs;
using Microsoft.Extensions.Logging;

namespace PayQueue.Application.Jobs;

public sealed class SampleHeartbeatJob(ILogger<SampleHeartbeatJob> logger) : ISampleHeartbeatJob
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Sample heartbeat job executed at {UtcNow}", DateTimeOffset.UtcNow);
        return Task.CompletedTask;
    }
}


