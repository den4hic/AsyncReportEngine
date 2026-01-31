namespace AsyncReportEngine.Shared.Entities;

public class Address
{
    public int Id { get; set; }
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
}
