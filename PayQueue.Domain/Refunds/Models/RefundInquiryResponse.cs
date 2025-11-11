namespace PayQueue.Domain.Refunds.Models;

public sealed record RefundInquiryResponse(
    bool Success,
    string ResponseCode,
    string ResponseMessage,
    IReadOnlyCollection<RefundInquiryItem> RefundTransactionResponseList);

public sealed record RefundInquiryItem(
    long Id,
    long RefundId,
    long TransferId,
    string TransferDateTime,
    long InternalReferenceId,
    decimal Amount,
    string Pan,
    string TransactionDate,
    string TransactionTime,
    string Status,
    string StatusDescription,
    string StatusDetailCode,
    string StatusDetailDesc,
    long TerminalId,
    int Stan);


