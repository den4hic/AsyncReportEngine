using AsyncReportEngine.Shared.Dtos.Products;
using AsyncReportEngine.Shared.Entities;
using AutoMapper;

namespace AsyncReportEngine.Shared.MappingProfiles;

public class CatalogProfile : Profile
{
    public CatalogProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName,
                       opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Без категорії"));
    }
}
