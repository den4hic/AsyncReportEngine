namespace AsyncReportEngine.Shared.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        public List<ProductReview> Reviews { get; set; } = new();
    }
}
