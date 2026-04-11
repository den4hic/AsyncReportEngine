using AsyncReportEngine.Shared.Entities;

namespace AsyncReportEngine.DataAccess.Abstraction.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<int?> GetIdByNameAsync(string name);
}
