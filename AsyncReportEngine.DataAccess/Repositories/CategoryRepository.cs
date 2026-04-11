using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.DataAccess.Context;
using AsyncReportEngine.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace AsyncReportEngine.DataAccess.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ReportDbContext context) : base(context) { }

    public async Task<int?> GetIdByNameAsync(string name)
    {
        var category = await context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name == name);
        return category?.Id;
    }
}
