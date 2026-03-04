using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos.Orders;
using AutoMapper;

namespace AsyncReportEngine.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository orderRepository;
    private readonly IMapper mapper;

    public OrderService(IOrderRepository orderRepository, IMapper mapper)
    {
        this.orderRepository = orderRepository;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<OrderDto>> GetRecentOrdersAsync(int take = 10)
    {
        var orders = await orderRepository.GetRecentOrdersAsync(take);
        return mapper.Map<IEnumerable<OrderDto>>(orders);
    }
}
