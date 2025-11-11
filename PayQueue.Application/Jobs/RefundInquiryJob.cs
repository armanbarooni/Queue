using PayQueue.Domain.Refunds;
using Microsoft.Extensions.Logging;

namespace PayQueue.Application.Jobs;

public sealed class RefundInquiryJob(
    IRefundInquiryService refundInquiryService,
    ILogger<RefundInquiryJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting refund inquiry job.");
        await refundInquiryService.ProcessRefundUpdatesAsync(cancellationToken);
    }
}


