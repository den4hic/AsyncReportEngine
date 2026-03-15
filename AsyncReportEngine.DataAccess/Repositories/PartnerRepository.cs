using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.DataAccess.Context;
using AsyncReportEngine.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace AsyncReportEngine.DataAccess.Repositories;

public class PartnerRepository : IPartnerRepository
{
    private readonly ReportDbContext context;

    public PartnerRepository(ReportDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await context.Customers.OrderBy(p => p.FirstName).ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await context.Customers.FindAsync(id);
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await context.Customers.CountAsync();
    }
}
