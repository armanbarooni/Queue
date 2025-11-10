using System.Net.Http.Json;
using TandemQueue.Domain.Refunds;
using TandemQueue.Domain.Refunds.Models;
using TandemQueue.Shared.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TandemQueue.Infrastructure.Refunds;

internal sealed class RefundInquiryHttpClient(
    HttpClient httpClient,
    IOptions<RefundInquiryOptions> options,
    ILogger<RefundInquiryHttpClient> logger) : IRefundInquiryClient
{
    private readonly RefundInquiryOptions _options = options.Value;

    public async Task<RefundInquiryResponse?> GetRefundInquiryAsync(long baseTransactionId, CancellationToken cancellationToken)
    {
        var effectiveBaseId = baseTransactionId == 0
            ? _options.DefaultBaseTransactionId
            : baseTransactionId;

        if (effectiveBaseId is null)
        {
            logger.LogWarning("Base transaction id is not provided for refund inquiry.");
            return null;
        }

        var requestUrl = $"{_options.Endpoint.TrimEnd('/')}/{effectiveBaseId}";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                request.Headers.Add("X-API-KEY", _options.ApiKey);
            }

            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<RefundInquiryResponse>(cancellationToken: cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling refund inquiry API for base transaction {TransactionId}", effectiveBaseId);
            return null;
        }
    }
}


