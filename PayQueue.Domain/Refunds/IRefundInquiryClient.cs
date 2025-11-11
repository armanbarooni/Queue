using PayQueue.Domain.Refunds.Models;

namespace PayQueue.Domain.Refunds;

public interface IRefundInquiryClient
{
    Task<RefundInquiryResponse?> GetRefundInquiryAsync(long baseTransactionId, CancellationToken cancellationToken);
}


