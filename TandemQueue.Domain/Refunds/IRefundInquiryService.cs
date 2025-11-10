namespace TandemQueue.Domain.Refunds;

public interface IRefundInquiryService
{
    Task ProcessRefundUpdatesAsync(CancellationToken cancellationToken);
}


