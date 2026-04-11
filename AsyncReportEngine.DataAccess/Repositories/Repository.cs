using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace AsyncReportEngine.DataAccess.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ReportDbContext context;
    public Repository(ReportDbContext context) => this.context = context;

    public async Task<IEnumerable<T>> GetAllAsync() => await context.Set<T>().AsNoTracking().ToListAsync();
    public async Task<T?> GetByIdAsync(int id) => await context.Set<T>().FindAsync(id);
    public async Task AddAsync(T entity) => await context.Set<T>().AddAsync(entity);
    public void Update(T entity) => context.Set<T>().Update(entity);
    public void Delete(T entity) => context.Set<T>().Remove(entity);
    public async Task<bool> SaveChangesAsync() => await context.SaveChangesAsync() > 0;
}
