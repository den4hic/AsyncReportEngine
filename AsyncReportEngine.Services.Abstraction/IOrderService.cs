using AsyncReportEngine.Shared.Dtos.Orders;

namespace AsyncReportEngine.Services.Abstraction;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetRecentOrdersAsync(int take = 10);
}
