using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.DataAccess.Context;
using AsyncReportEngine.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace AsyncReportEngine.DataAccess.Repositories;

public class SupplierRepository : Repository<Supplier>, ISupplierRepository
{
    public SupplierRepository(ReportDbContext context) : base(context) { }

    public async Task<int?> GetIdByNameAsync(string name)
    {
        var supplier = await context.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CompanyName == name);
        return supplier?.Id;
    }
}
