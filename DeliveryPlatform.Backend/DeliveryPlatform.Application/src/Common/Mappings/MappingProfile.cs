using AutoMapper;
using DeliveryPlatform.Application.src.Features.Category.GetByBusiness;
using DeliveryPlatform.Application.src.Features.Category.GetById;
using DeliveryPlatform.Application.src.Features.Product.GetByBusiness;
using DeliveryPlatform.Application.src.Features.Product.GetById;
using DeliveryPlatform.Domain.src.Entity;

namespace DeliveryPlatform.Application.src.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, CategoryResponse>();
        CreateMap<Category, CategoryByIdResponse>();

        CreateMap<Product, ProductResponse>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty));
        CreateMap<Product, ProductByIdResponse>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty));
    }
}
