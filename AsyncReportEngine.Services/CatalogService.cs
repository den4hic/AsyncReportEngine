using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos.Dashboards;
using AsyncReportEngine.Shared.Dtos.Products;
using AutoMapper;

namespace AsyncReportEngine.Services;
public class CatalogService : ICatalogService
{
    private readonly IProductRepository productRepository;
    private readonly IDashboardRepository dashboardRepository;
    private readonly IMapper mapper;

    public CatalogService(IProductRepository productRepository, IDashboardRepository dashboardRepository, IMapper mapper)
    {
        this.productRepository = productRepository;
        this.dashboardRepository = dashboardRepository;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsync(int page, int pageSize)
    {
        int skip = (page - 1) * pageSize;
        var products = await productRepository.GetProductsAsync(skip, pageSize);

        return mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product == null) return null;

        return mapper.Map<ProductDto>(product);
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        return new DashboardStatsDto
        {
            TotalOrders = await dashboardRepository.GetTotalOrdersCountAsync(),
            TotalCustomers = await dashboardRepository.GetTotalCustomersCountAsync(),
            TotalProducts = await dashboardRepository.GetTotalProductsCountAsync(),
            TotalRevenue = await dashboardRepository.GetTotalSuccessfulRevenueAsync()
        };
    }
}
