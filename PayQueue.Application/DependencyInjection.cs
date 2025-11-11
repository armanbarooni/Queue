using FluentValidation;
using PayQueue.Application.Jobs;
using PayQueue.Application.Services;
using PayQueue.Domain.Jobs;
using PayQueue.Domain.Refunds;
using PayQueue.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace PayQueue.Application;

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



