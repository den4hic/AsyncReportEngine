using AsyncReportEngine.Shared.Enum;

namespace AsyncReportEngine.Shared.Entities;

public class ReportRequest
{
    public Guid Id { get; set; }
    public ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string? FileUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? StartedAt { get; set; }
    public int? DurationMs { get; set; }
}
