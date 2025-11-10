using Hangfire;
using TandemQueue.Domain.Scheduling;
using TandemQueue.Infrastructure.Configuration;
using TandemQueue.Shared.Configuration;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TandemQueue.Infrastructure.Scheduling;

public static class HangfireServiceCollectionExtensions
{
    public static IServiceCollection AddHangfireInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructureOptions(configuration);

        services.AddHangfire((serviceProvider, configurationBuilder) =>
        {
            var sqlOptions = serviceProvider.GetRequiredService<IOptions<SqlServerOptions>>().Value;

            configurationBuilder
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(sqlOptions.ConnectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromSeconds(sqlOptions.CommandBatchMaxTimeoutSeconds),
                    SlidingInvisibilityTimeout = TimeSpan.FromSeconds(sqlOptions.SlidingInvisibilityTimeoutSeconds),
                    QueuePollInterval = TimeSpan.FromSeconds(sqlOptions.QueuePollIntervalSeconds),
                    SchemaName = "Hangfire"
                });
        });

        services.AddScoped<IJobScheduler, HangfireJobScheduler>();

        return services;
    }

    public static IServiceCollection AddConfiguredHangfireServer(this IServiceCollection services)
    {
        services.AddHangfireServer((serviceProvider, options) =>
        {
            var hangfireOptions = serviceProvider.GetRequiredService<IOptions<HangfireOptions>>().Value;
            options.WorkerCount = hangfireOptions.WorkerCount;
            options.Queues = hangfireOptions.Queues;
        });

        return services;
    }
}


