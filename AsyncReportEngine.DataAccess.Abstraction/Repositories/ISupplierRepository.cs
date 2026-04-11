using AsyncReportEngine.Shared.Entities;

namespace AsyncReportEngine.DataAccess.Abstraction.Repositories;

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<int?> GetIdByNameAsync(string name);
}
