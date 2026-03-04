using AsyncReportEngine.Shared.Entities;

namespace AsyncReportEngine.DataAccess.Abstraction.Repositories;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetRecentOrdersAsync(int take);
}
