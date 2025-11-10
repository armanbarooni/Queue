namespace TandemQueue.Domain.Refunds.Models;

public sealed record RefundStatusUpdate(
    long RefundRowId,
    int RefundState,
    string? BodyResponse);


