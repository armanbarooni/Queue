using TandemQueue.Domain.Refunds.Models;

namespace TandemQueue.Domain.Refunds;

public interface IRefundInquiryClient
{
    Task<RefundInquiryResponse?> GetRefundInquiryAsync(long baseTransactionId, CancellationToken cancellationToken);
}


