using System.Linq.Expressions;

namespace PayQueue.Domain.Scheduling;

public interface IJobScheduler
{
    string Enqueue<TJob>(Expression<Func<TJob, Task>> methodCall)
        where TJob : class;

    void ScheduleRecurring<TJob>(string recurringJobId, Expression<Func<TJob, Task>> methodCall, string cronExpression)
        where TJob : class;
}


