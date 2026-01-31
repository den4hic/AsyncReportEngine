namespace AsyncReportEngine.Shared.Entities;

public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string? IdentityUserId { get; set; }

    public List<Address> Addresses { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
    public List<ProductReview> Reviews { get; set; } = new();
}
