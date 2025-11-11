namespace PayQueue.Shared.Configuration;

public sealed class SqlServerOptions
{
    public const string SectionName = "SqlServer";

    public string ConnectionString { get; set; } = string.Empty;

    public int CommandBatchMaxTimeoutSeconds { get; set; } = 30;

    public int SlidingInvisibilityTimeoutSeconds { get; set; } = 30;

    public int QueuePollIntervalSeconds { get; set; } = 15;
}


