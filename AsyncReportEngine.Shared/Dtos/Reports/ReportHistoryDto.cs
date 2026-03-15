namespace AsyncReportEngine.Shared.Dtos.Reports;

public class ReportHistoryDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public string? ErrorMessage { get; set; }
}
