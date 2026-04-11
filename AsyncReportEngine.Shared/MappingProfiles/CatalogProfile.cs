using AsyncReportEngine.Shared.Dtos.Products;
using AsyncReportEngine.Shared.Entities;
using AutoMapper;

namespace AsyncReportEngine.Shared.MappingProfiles;

public class CatalogProfile : Profile
{
    public CatalogProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier.CompanyName))
            .ForMember(dest => dest.SupplierId, opt => opt.MapFrom(src => src.SupplierId))
            .ReverseMap()
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore());
    }
}
