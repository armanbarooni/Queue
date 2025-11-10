namespace TandemQueue.Shared.Configuration;

public sealed class RefundInquiryOptions
{
    public const string SectionName = "RefundInquiry";

    public string ConnectionString { get; set; } = string.Empty;

    public string Endpoint { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public long? DefaultBaseTransactionId { get; set; }
}


