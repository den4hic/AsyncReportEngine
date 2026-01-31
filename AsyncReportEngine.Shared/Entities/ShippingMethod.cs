namespace AsyncReportEngine.Shared.Entities;

public class ShippingMethod
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int EstimatedDays { get; set; }
}
