namespace AsyncReportEngine.DataAccess.Abstraction.Repositories;

public interface IDashboardRepository
{
    Task<int> GetTotalOrdersCountAsync();
    Task<int> GetTotalCustomersCountAsync();
    Task<int> GetTotalProductsCountAsync();
    Task<decimal> GetTotalSuccessfulRevenueAsync();
}
