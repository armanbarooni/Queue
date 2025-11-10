using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Microsoft.Extensions.Logging;
using TandemQueue.Domain.Refunds;
using TandemQueue.Domain.Refunds.Models;

namespace TandemQueue.Application.Services;

public sealed class RefundInquiryService(
    IRefundRepository refundRepository,
    IRefundInquiryClient refundInquiryClient,
    ILogger<RefundInquiryService> logger) : IRefundInquiryService
{
    public async Task ProcessRefundUpdatesAsync(CancellationToken cancellationToken)
    {
        var pendingRefunds = await refundRepository.GetPendingRefundsAsync(cancellationToken);

        if (pendingRefunds.Count == 0)
        {
            logger.LogInformation("No pending refund records found.");
            return;
        }

        var updates = new List<RefundStatusUpdate>();
        foreach (var group in pendingRefunds.GroupBy(r => r.BaseTransactionId))
        {
            var baseTransactionId = group.Key;
            var recordsForBase = group.ToList();
            var trackingLookup = BuildTrackingLookup(recordsForBase);

            try
            {
                var response = await refundInquiryClient.GetRefundInquiryAsync(baseTransactionId, cancellationToken);

                if (response is null)
                {
                    logger.LogWarning("No refund information returned for base transaction {BaseTransactionId}", baseTransactionId);
                    continue;
                }

                foreach (var item in response.RefundTransactionResponseList)
                {
                    if (!TryResolveRecord(item, trackingLookup, out var record))
                    {
                        logger.LogDebug("No matching refund record found for refund response {RefundId} on base transaction {BaseTransactionId}.", item.RefundId, baseTransactionId);
                        continue;
                    }

                    var newState = MapStatusToRefundState(item.Status);
                    if (newState is null)
                    {
                        logger.LogInformation("Refund {RefundId} has status {Status}; no state change applied.", item.RefundId, item.Status);
                        continue;
                    }

                    var bodyResponse = System.Text.Json.JsonSerializer.Serialize(item);
                    updates.Add(new RefundStatusUpdate(record.RefundRowId, newState.Value, bodyResponse));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process refund inquiry for base transaction {BaseTransactionId}", baseTransactionId);
            }
        }

        if (updates.Count > 0)
        {
            await refundRepository.UpdateRefundStatesAsync(updates, cancellationToken);
            logger.LogInformation("Updated refund states for {Count} records.", updates.Count);
        }
    }

    private static Dictionary<string, RefundRecord> BuildTrackingLookup(IEnumerable<RefundRecord> records) =>
        records
            .Where(r => !string.IsNullOrWhiteSpace(r.RefundTransactionId))
            .GroupBy(r => r.RefundTransactionId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

    private static bool TryResolveRecord(
        RefundInquiryItem item,
        IReadOnlyDictionary<string, RefundRecord> trackingLookup,
        out RefundRecord? record)
    {
        var key = item.RefundId.ToString(CultureInfo.InvariantCulture);
        return trackingLookup.TryGetValue(key, out record);
    }

    private static int? MapStatusToRefundState(string? status) =>
        status switch
        {
            "000" => 2,
            "001" => 1,
            "004" => 3,
            _ => null
        };
}



