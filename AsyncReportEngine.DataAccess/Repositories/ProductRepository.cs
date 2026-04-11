using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.DataAccess.Context;
using AsyncReportEngine.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace AsyncReportEngine.DataAccess.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ReportDbContext context) : base(context) { }

    public async Task<IEnumerable<Product>> GetPagedProductsAsync(int skip, int take)
    {
        return await context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .OrderBy(p => p.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync() => await context.Products.CountAsync();
}
