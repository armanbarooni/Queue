using FluentValidation;
using TandemQueue.Application.Jobs;
using TandemQueue.Application.Services;
using TandemQueue.Domain.Jobs;
using TandemQueue.Domain.Refunds;
using TandemQueue.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TandemQueue.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ISampleHeartbeatJob, SampleHeartbeatJob>();
        services.AddScoped<SampleHeartbeatJob>();
        services.AddScoped<RefundInquiryJob>();
        services.AddScoped<IRefundInquiryService, RefundInquiryService>();
        services.AddScoped<IHeartbeatJobService, HeartbeatJobService>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}



