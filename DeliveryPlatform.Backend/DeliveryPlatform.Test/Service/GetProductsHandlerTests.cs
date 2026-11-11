using AutoMapper;
using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Exceptions;
using DeliveryPlatform.Application.src.Features.Product.GetByBusiness;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class GetProductsHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetProductsHandler _handler;

    public GetProductsHandlerTests()
    {
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _mapperMock = new Mock<IMapper>();

        _mapperMock
            .Setup(m => m.Map<ProductResponse>(It.IsAny<Product>()))
            .Returns((Product product) => new ProductResponse(
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

        _handler = new GetProductsHandler(
            _businessRepositoryMock.Object,
            _productRepositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedProducts_WhenUserOwnsBusiness()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _productRepositoryMock
            .Setup(r => r.GetByBusinessIdAsync(businessId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>
            {
                CreateProduct(businessId, "A"),
                CreateProduct(businessId, "B"),
                CreateProduct(businessId, "C")
            });

        var result = await _handler.Handle(
            new GetProductsQuery(businessId, userId, false, null, 1, 2),
            CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal("A", result.Items[0].Name);
    }

    [Fact]
    public async Task Handle_ShouldPassCategoryFilter()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _productRepositoryMock
            .Setup(r => r.GetByBusinessIdAsync(businessId, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        await _handler.Handle(
            new GetProductsQuery(businessId, userId, false, categoryId),
            CancellationToken.None);

        _productRepositoryMock.Verify(
            r => r.GetByBusinessIdAsync(businessId, categoryId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSkipOwnershipCheck_WhenIsAdmin()
    {
        var businessId = Guid.NewGuid();

        _productRepositoryMock
            .Setup(r => r.GetByBusinessIdAsync(businessId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        await _handler.Handle(
            new GetProductsQuery(businessId, Guid.NewGuid(), true),
            CancellationToken.None);

        _businessRepositoryMock.Verify(
            r => r.GetByUserIdAndBusinessIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenBusinessNotFound()
    {
        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new GetProductsQuery(Guid.NewGuid(), Guid.NewGuid(), false), CancellationToken.None));

        _productRepositoryMock.Verify(
            r => r.GetByBusinessIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Product CreateProduct(Guid businessId, string name) => new()
    {
        Id = Guid.NewGuid(),
        BusinessId = businessId,
        CategoryId = Guid.NewGuid(),
        Name = name,
        Price = 10m,
        IsAvailable = true,
        Category = new Category { Id = Guid.NewGuid(), BusinessId = businessId, Name = "Food" }
    };
}
