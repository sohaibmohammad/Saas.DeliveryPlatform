using AutoMapper;
using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Exceptions;
using DeliveryPlatform.Application.src.Features.Product.GetById;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class GetProductByIdHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetProductByIdHandler _handler;

    public GetProductByIdHandlerTests()
    {
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _mapperMock = new Mock<IMapper>();

        _mapperMock
            .Setup(m => m.Map<ProductByIdResponse>(It.IsAny<Product>()))
            .Returns((Product product) => new ProductByIdResponse(
                product.Id,
                product.BusinessId,
                product.CategoryId,
                product.Category?.Name ?? string.Empty,
                product.Name,
                product.Description,
                product.Price,
                product.IsAvailable,
                product.CreatedAt,
                product.UpdatedAt));

        _handler = new GetProductByIdHandler(
            _businessRepositoryMock.Object,
            _productRepositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnProduct_WhenFound()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            BusinessId = businessId,
            CategoryId = Guid.NewGuid(),
            Name = "Burger",
            Price = 10m,
            IsAvailable = true,
            Category = new Category { Name = "Food" }
        };

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _productRepositoryMock
            .Setup(r => r.GetByBusinessIdAndProductIdAsync(businessId, productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _handler.Handle(
            new GetProductByIdQuery(businessId, productId, userId, false),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(productId, result!.Id);
        Assert.Equal("Burger", result.Name);
        Assert.Equal("Food", result.CategoryName);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenProductNotFound()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _productRepositoryMock
            .Setup(r => r.GetByBusinessIdAndProductIdAsync(businessId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await _handler.Handle(
            new GetProductByIdQuery(businessId, Guid.NewGuid(), userId, false),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenBusinessNotFound()
    {
        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(
                new GetProductByIdQuery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), false),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldSkipOwnershipCheck_WhenIsAdmin()
    {
        var businessId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _productRepositoryMock
            .Setup(r => r.GetByBusinessIdAndProductIdAsync(businessId, productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { Id = productId, BusinessId = businessId, Name = "Burger", Category = new Category { Name = "Food" } });

        var result = await _handler.Handle(
            new GetProductByIdQuery(businessId, productId, Guid.NewGuid(), true),
            CancellationToken.None);

        Assert.NotNull(result);
        _businessRepositoryMock.Verify(
            r => r.GetByUserIdAndBusinessIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
