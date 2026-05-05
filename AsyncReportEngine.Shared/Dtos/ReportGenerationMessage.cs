namespace AsyncReportEngine.Shared.Dtos;

public class ReportGenerationMessage
{
    public Guid RequestId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string UserEmail { get; set; } = string.Empty;

    public Guid? BenchmarkRunId { get; set; }
    public int? PartnerId { get; set; }
}
