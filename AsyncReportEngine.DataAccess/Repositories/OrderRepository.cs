using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.DataAccess.Context;
using AsyncReportEngine.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace AsyncReportEngine.DataAccess.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ReportDbContext context;

    public OrderRepository(ReportDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int take)
    {
        return await context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderDate)
            .Take(take)
            .ToListAsync();
    }
}