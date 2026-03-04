using AsyncReportEngine.Shared.Entities;

namespace AsyncReportEngine.DataAccess.Abstraction.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetProductsAsync(int skip, int take);
    Task<Product?> GetByIdAsync(int id);
    Task<int> GetTotalCountAsync();
}
