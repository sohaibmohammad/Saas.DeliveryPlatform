using AutoMapper;
using DeliveryPlatform.Application.src.Features.Category.GetByBusiness;
using DeliveryPlatform.Application.src.Features.Category.GetById;
using DeliveryPlatform.Application.src.Features.Product.GetByBusiness;
using DeliveryPlatform.Application.src.Features.Product.GetById;
using DeliveryPlatform.Domain.src.Entity;
using DeliveryPlatform.Test.TestHelpers;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class MappingProfileTests
{
    private readonly IMapper _mapper = TestMapper.Create();

    [Fact]
    public void Configuration_ShouldBeValid()
    {
        var configuration = new MapperConfiguration(
            cfg => cfg.AddProfile<DeliveryPlatform.Application.src.Common.Mappings.MappingProfile>(),
            Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_ShouldMapCategoryToCategoryResponse()
    {
        var category = new Category { Id = Guid.NewGuid(), BusinessId = Guid.NewGuid(), Name = "Drinks" };

        var result = _mapper.Map<CategoryResponse>(category);

        Assert.Equal(category.Id, result.Id);
        Assert.Equal(category.BusinessId, result.BusinessId);
        Assert.Equal(category.Name, result.Name);
    }

    [Fact]
    public void Map_ShouldMapCategoryToCategoryByIdResponse()
    {
        var category = new Category { Id = Guid.NewGuid(), BusinessId = Guid.NewGuid(), Name = "Drinks" };

        var result = _mapper.Map<CategoryByIdResponse>(category);

        Assert.Equal(category.Id, result.Id);
        Assert.Equal(category.Name, result.Name);
    }

    [Fact]
    public void Map_ShouldMapProductWithCategoryName()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            BusinessId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Name = "Burger",
            Description = "Tasty",
            Price = 12.5m,
            IsAvailable = true,
            Category = new Category { Name = "Food" }
        };

        var result = _mapper.Map<ProductResponse>(product);

        Assert.Equal(product.Id, result.Id);
        Assert.Equal(product.Name, result.Name);
        Assert.Equal("Food", result.CategoryName);
        Assert.Equal(product.Price, result.Price);
    }

    [Fact]
    public void Map_ShouldMapProductByIdWithEmptyCategoryName_WhenCategoryIsNull()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            BusinessId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Name = "Burger",
            Category = null!
        };

        var result = _mapper.Map<ProductByIdResponse>(product);

        Assert.Equal(string.Empty, result.CategoryName);
    }
}
