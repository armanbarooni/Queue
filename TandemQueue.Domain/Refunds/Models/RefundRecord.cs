namespace TandemQueue.Domain.Refunds.Models;

public sealed record RefundRecord(
    long RefundRowId,
    string RefundTransactionId,
    long BaseTransactionId);


