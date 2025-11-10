namespace TandemQueue.Shared.Configuration;

public sealed class HangfireOptions
{
    public const string SectionName = "Hangfire";

    /// <summary>
    /// The queues that this service should process.
    /// </summary>
    public string[] Queues { get; set; } = ["default"];

    /// <summary>
    /// Controls the worker count for the Hangfire server instances.
    /// </summary>
    public int WorkerCount { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// When true, enables the Hangfire dashboard middleware.
    /// </summary>
    public bool EnableDashboard { get; set; } = true;

    /// <summary>
    /// Optional route for the dashboard endpoint.
    /// </summary>
    public string DashboardPath { get; set; } = "/hangfire";
}


