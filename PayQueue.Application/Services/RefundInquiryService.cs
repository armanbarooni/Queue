using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Microsoft.Extensions.Logging;
using PayQueue.Domain.Refunds;
using PayQueue.Domain.Refunds.Models;

namespace PayQueue.Application.Services;

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

                    if (record is null)
                    {
                        // Defensive null-check for static analysis; should not happen when TryResolveRecord returns true
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

    private static Dictionary<long, RefundRecord> BuildTrackingLookup(IEnumerable<RefundRecord> records)
    {
        var map = new Dictionary<long, RefundRecord>();

        foreach (var record in records)
        {
            if (!TryParseLong(record.RefundTransactionId, out var numericId))
            {
                continue;
            }

            if (!map.ContainsKey(numericId))
            {
                map[numericId] = record;
            }
        }

        return map;
    }

    private static bool TryResolveRecord(
        RefundInquiryItem item,
        IReadOnlyDictionary<long, RefundRecord> trackingLookup,
        out RefundRecord? record)
    {
        record = null;
        return trackingLookup.TryGetValue(item.RefundId, out record);
    }

    private static bool TryParseLong(string? value, out long result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Normalize: keep only ASCII digits to handle trailing spaces or formatting
        var digitsOnly = new string(value.Where(char.IsDigit).ToArray());
        return long.TryParse(digitsOnly, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
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



