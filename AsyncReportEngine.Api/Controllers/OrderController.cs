using AsyncReportEngine.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace AsyncReportEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService orderService;

    public OrdersController(IOrderService orderService)
    {
        this.orderService = orderService;
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentOrders([FromQuery] int take = 10)
    {
        var orders = await orderService.GetRecentOrdersAsync(take);
        return Ok(orders);
    }
}
