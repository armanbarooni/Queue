using PayQueue.Shared.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PayQueue.Infrastructure.Configuration;

internal static class InfrastructureOptionsExtensions
{
    public static IServiceCollection AddInfrastructureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SqlServerOptions>()
            .Bind(configuration.GetSection(SqlServerOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), $"{SqlServerOptions.SectionName}:ConnectionString must be provided.")
            .ValidateOnStart();

        services.AddOptions<HangfireOptions>()
            .Bind(configuration.GetSection(HangfireOptions.SectionName))
            .Validate(options => options.Queues.Length > 0, $"{HangfireOptions.SectionName}:Queues must contain at least one entry.")
            .Validate(options => options.WorkerCount > 0, $"{HangfireOptions.SectionName}:WorkerCount must be greater than zero.")
            .ValidateOnStart();

        return services;
    }
}


