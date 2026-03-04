namespace AsyncReportEngine.Shared.Dtos.Products;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
}
