using AsyncReportEngine.Shared.Dtos.Orders;
using AsyncReportEngine.Shared.Entities;
using AutoMapper;

namespace AsyncReportEngine.Shared.MappingProfiles;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.CustomerName,
                       opt => opt.MapFrom(src => src.Customer != null ? $"{src.Customer.FirstName} {src.Customer.LastName}" : "Невідомий клієнт"));
    }
}
