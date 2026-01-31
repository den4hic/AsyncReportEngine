namespace AsyncReportEngine.Shared.Entities;

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? ShippingMethodId { get; set; }
    public ShippingMethod? ShippingMethod { get; set; }

    public List<PaymentTransaction> Transactions { get; set; } = new();

    public List<OrderItem> Items { get; set; } = new();
}
