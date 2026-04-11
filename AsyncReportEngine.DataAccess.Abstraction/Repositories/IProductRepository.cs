using AsyncReportEngine.Shared.Entities;

namespace AsyncReportEngine.DataAccess.Abstraction.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetPagedProductsAsync(int skip, int take);
    Task<int> GetTotalCountAsync();
}
