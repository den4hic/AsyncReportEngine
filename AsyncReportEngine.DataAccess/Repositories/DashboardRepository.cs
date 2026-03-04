using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace AsyncReportEngine.DataAccess.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly ReportDbContext context;

    public DashboardRepository(ReportDbContext context)
    {
        this.context = context;
    }

    public async Task<int> GetTotalOrdersCountAsync() => await context.Orders.CountAsync();
    public async Task<int> GetTotalCustomersCountAsync() => await context.Customers.CountAsync();
    public async Task<int> GetTotalProductsCountAsync() => await context.Products.CountAsync();

    public async Task<decimal> GetTotalSuccessfulRevenueAsync()
    {
        return await context.PaymentTransactions
            .Where(t => t.Status == "Success")
            .SumAsync(t => t.Amount);
    }
}
