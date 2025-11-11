using PayQueue.Infrastructure.Configuration;
using PayQueue.Shared.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace PayQueue.Infrastructure.Monitoring;

public static class MonitoringServiceCollectionExtensions
{
    public static IServiceCollection AddMonitoringInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructureOptions(configuration);

        services.AddHealthChecks()
            .AddSqlServer(
                sp => sp.GetRequiredService<IOptions<SqlServerOptions>>().Value.ConnectionString,
                name: "sqlserver",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready"]);

        return services;
    }
}


