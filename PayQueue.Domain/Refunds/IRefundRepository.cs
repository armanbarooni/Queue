using PayQueue.Domain.Refunds.Models;

namespace PayQueue.Domain.Refunds;

public interface IRefundRepository
{
    Task<IReadOnlyCollection<RefundRecord>> GetPendingRefundsAsync(CancellationToken cancellationToken);

    Task UpdateRefundStatesAsync(IEnumerable<RefundStatusUpdate> updates, CancellationToken cancellationToken);
}


