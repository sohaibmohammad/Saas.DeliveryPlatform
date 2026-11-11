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
            .ForCtorParam(nameof(ProductResponse.CategoryName), opt => opt.MapFrom(src => src.Category == null ? string.Empty : src.Category.Name));
        CreateMap<Product, ProductByIdResponse>()
            .ForCtorParam(nameof(ProductByIdResponse.CategoryName), opt => opt.MapFrom(src => src.Category == null ? string.Empty : src.Category.Name));
    }
}
