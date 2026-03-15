using AsyncReportEngine.Shared.Dtos.Partners;
using AsyncReportEngine.Shared.Entities;
using AutoMapper;

namespace AsyncReportEngine.Shared.MappingProfiles;

public class PartnerProfile : Profile
{
    public PartnerProfile()
    {
        CreateMap<Customer, PartnerDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
            .ForMember(dest => dest.ContactEmail, opt => opt.MapFrom(src => src.Email ?? string.Empty));


    }
}
