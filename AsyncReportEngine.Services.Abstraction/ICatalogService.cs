using AsyncReportEngine.Shared.Dtos.Basic;
using AsyncReportEngine.Shared.Dtos.Dashboards;
using AsyncReportEngine.Shared.Dtos.Pagination;
using AsyncReportEngine.Shared.Dtos.Products;

namespace AsyncReportEngine.Services.Abstraction;

public interface ICatalogService
{
    Task<PagedResult<ProductDto>> GetProductsAsync(int page, int pageSize);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<DashboardStatsDto> GetDashboardStatsAsync();

    Task<ProductDto> CreateProductAsync(ProductDto productDto);
    Task<bool> UpdateProductAsync(int id, ProductDto productDto);
    Task<bool> DeleteProductAsync(int id);
    Task<IEnumerable<LookupDto>> GetCategoriesLookupAsync();
    Task<IEnumerable<LookupDto>> GetSuppliersLookupAsync();
}