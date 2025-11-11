using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PayQueue.Domain.Refunds;
using PayQueue.Shared.Configuration;

namespace PayQueue.Infrastructure.Refunds;

public static class RefundServiceCollectionExtensions
{
    public static IServiceCollection AddRefundInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RefundInquiryOptions>()
            .Bind(configuration.GetSection(RefundInquiryOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), $"{RefundInquiryOptions.SectionName}:ConnectionString must be provided.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Endpoint), $"{RefundInquiryOptions.SectionName}:Endpoint must be provided.")
            .ValidateOnStart();

        services.AddScoped<IRefundRepository, SqlRefundRepository>();
        services.AddHttpClient<IRefundInquiryClient, RefundInquiryHttpClient>()
            .ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<IOptions<RefundInquiryOptions>>().Value;
                var handler = new HttpClientHandler();

                if (options.AllowInvalidCertificates)
                {
                    handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }

                return handler;
            });

        return services;
    }
}


