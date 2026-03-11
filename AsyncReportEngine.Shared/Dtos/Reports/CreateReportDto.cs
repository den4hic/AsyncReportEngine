namespace AsyncReportEngine.Shared.Dtos.Reports;

public class CreateReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Email { get; set; }

    public List<int>? PartnerIds { get; set; }
}
