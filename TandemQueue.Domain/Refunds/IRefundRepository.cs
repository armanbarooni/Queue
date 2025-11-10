using TandemQueue.Domain.Refunds.Models;

namespace TandemQueue.Domain.Refunds;

public interface IRefundRepository
{
    Task<IReadOnlyCollection<RefundRecord>> GetPendingRefundsAsync(CancellationToken cancellationToken);

    Task UpdateRefundStatesAsync(IEnumerable<RefundStatusUpdate> updates, CancellationToken cancellationToken);
}


