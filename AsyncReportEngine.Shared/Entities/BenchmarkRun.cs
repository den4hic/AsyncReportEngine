namespace AsyncReportEngine.Shared.Entities;

public class BenchmarkRun
{
    public Guid Id { get; set; }
    public string Approach { get; set; } = string.Empty;
    public int RequestedCount { get; set; }
    public string Status { get; set; } = "Running";
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? TotalDurationMs { get; set; }
    public double? AvgDurationMs { get; set; }
    public double? ThroughputPerSec { get; set; }
}