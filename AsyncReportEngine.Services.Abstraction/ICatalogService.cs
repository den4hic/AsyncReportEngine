using AsyncReportEngine.Shared.Dtos.Dashboards;
using AsyncReportEngine.Shared.Dtos.Products;

namespace AsyncReportEngine.Services.Abstraction;

public interface ICatalogService
{
    Task<IEnumerable<ProductDto>> GetProductsAsync(int page, int pageSize);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}