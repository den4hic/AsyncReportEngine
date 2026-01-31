namespace AsyncReportEngine.Shared.Entities;

public class PaymentTransaction
{
    public Guid Id { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Success";

    public int OrderId { get; set; }
    public Order? Order { get; set; }
}
