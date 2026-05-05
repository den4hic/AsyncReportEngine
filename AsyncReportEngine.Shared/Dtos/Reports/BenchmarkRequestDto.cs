namespace AsyncReportEngine.Shared.Dtos.Reports;

public class BenchmarkRequestDto
{
    public string Approach { get; set; } = string.Empty;
    public List<int> PartnerIds { get; set; } = [];
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Email { get; set; }
}
