using System.Linq.Expressions;
using Hangfire;
using TandemQueue.Domain.Scheduling;

namespace TandemQueue.Infrastructure.Scheduling;

internal sealed class HangfireJobScheduler(
    IBackgroundJobClient backgroundJobClient,
    IRecurringJobManager recurringJobManager) : IJobScheduler
{
    public string Enqueue<TJob>(Expression<Func<TJob, Task>> methodCall)
        where TJob : class
        => backgroundJobClient.Enqueue(methodCall);

    public void ScheduleRecurring<TJob>(string recurringJobId, Expression<Func<TJob, Task>> methodCall, string cronExpression)
        where TJob : class
        => recurringJobManager.AddOrUpdate(recurringJobId, methodCall, cronExpression);
}


