namespace AsyncReportEngine.Shared.Entities;

public class ProductReview
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
}
