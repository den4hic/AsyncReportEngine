using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.DataAccess.Context;
using AsyncReportEngine.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace AsyncReportEngine.DataAccess.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ReportDbContext context;

    public ProductRepository(ReportDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<Product>> GetProductsAsync(int skip, int take)
    {
        return await context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<int> GetTotalCountAsync() => await context.Products.CountAsync();
}
