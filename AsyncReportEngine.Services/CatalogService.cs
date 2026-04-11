using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos.Basic;
using AsyncReportEngine.Shared.Dtos.Dashboards;
using AsyncReportEngine.Shared.Dtos.Pagination;
using AsyncReportEngine.Shared.Dtos.Products;
using AsyncReportEngine.Shared.Entities;
using AutoMapper;

namespace AsyncReportEngine.Services;

public class CatalogService : ICatalogService
{
    private readonly IProductRepository productRepository;
    private readonly IDashboardRepository dashboardRepository;
    private readonly ICategoryRepository categoryRepository;
    private readonly ISupplierRepository supplierRepository;
    private readonly IMapper mapper;

    public CatalogService(
        IProductRepository productRepository,
        IDashboardRepository dashboardRepository,
        ICategoryRepository categoryRepository,
        ISupplierRepository supplierRepository,
        IMapper mapper)
    {
        this.productRepository = productRepository;
        this.dashboardRepository = dashboardRepository;
        this.categoryRepository = categoryRepository;
        this.supplierRepository = supplierRepository;
        this.mapper = mapper;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(int page, int pageSize)
    {
        var totalCount = await productRepository.GetTotalCountAsync();

        int skip = (page - 1) * pageSize;
        var products = await productRepository.GetPagedProductsAsync(skip, pageSize);

        return new PagedResult<ProductDto>
        {
            Items = mapper.Map<List<ProductDto>>(products),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await productRepository.GetByIdAsync(id);
        return product == null ? null : mapper.Map<ProductDto>(product);
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

    public async Task<ProductDto> CreateProductAsync(ProductDto productDto)
    {
        var product = mapper.Map<Product>(productDto);

        await productRepository.AddAsync(product);
        await productRepository.SaveChangesAsync();

        return mapper.Map<ProductDto>(product);
    }

    public async Task<bool> UpdateProductAsync(int id, ProductDto productDto)
    {
        var existingProduct = await productRepository.GetByIdAsync(id);
        if (existingProduct == null) return false;

        mapper.Map(productDto, existingProduct);

        productRepository.Update(existingProduct);
        return await productRepository.SaveChangesAsync();
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product == null) return false;

        productRepository.Delete(product);
        return await productRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<LookupDto>> GetCategoriesLookupAsync()
    {
        var categories = await categoryRepository.GetAllAsync();
        return categories.Select(c => new LookupDto { Id = c.Id, Name = c.Name });
    }

    public async Task<IEnumerable<LookupDto>> GetSuppliersLookupAsync()
    {
        var suppliers = await supplierRepository.GetAllAsync();
        return suppliers.Select(s => new LookupDto { Id = s.Id, Name = s.CompanyName });
    }
}