using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Exceptions;
using DeliveryPlatform.Application.src.Features.Product.Create;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class CreateProductHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateProductHandler _handler;

    public CreateProductHandlerTests()
    {
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateProductHandler(
            _businessRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _productRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateProduct_WhenValid()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        Product? createdProduct = null;

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _categoryRepositoryMock
            .Setup(r => r.GetByBusinessIdAndCategoryIdAsync(businessId, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category { Id = categoryId, BusinessId = businessId, Name = "Food" });
        _productRepositoryMock
            .Setup(r => r.ExistsByNameInCategoryAsync(businessId, categoryId, "Burger", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _productRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((product, _) => createdProduct = product)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(
            new CreateProductCommand(businessId, categoryId, "  Burger  ", "  Tasty  ", 12.5m, true, userId),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(createdProduct);
        Assert.Equal(result, createdProduct!.Id);
        Assert.Equal(businessId, createdProduct.BusinessId);
        Assert.Equal(categoryId, createdProduct.CategoryId);
        Assert.Equal("Burger", createdProduct.Name);
        Assert.Equal("Tasty", createdProduct.Description);
        Assert.Equal(12.5m, createdProduct.Price);
        Assert.True(createdProduct.IsAvailable);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldClearWhitespaceDescription()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        Product? createdProduct = null;

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _categoryRepositoryMock
            .Setup(r => r.GetByBusinessIdAndCategoryIdAsync(businessId, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category { Id = categoryId, BusinessId = businessId });
        _productRepositoryMock
            .Setup(r => r.ExistsByNameInCategoryAsync(businessId, categoryId, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _productRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((product, _) => createdProduct = product)
            .Returns(Task.CompletedTask);

        await _handler.Handle(
            new CreateProductCommand(businessId, categoryId, "Burger", "   ", 1m, true, userId),
            CancellationToken.None);

        Assert.NotNull(createdProduct);
        Assert.Null(createdProduct!.Description);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenBusinessNotFound()
    {
        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(
                new CreateProductCommand(Guid.NewGuid(), Guid.NewGuid(), "Burger", null, 1m, true, Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCategoryNotFound()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _categoryRepositoryMock
            .Setup(r => r.GetByBusinessIdAndCategoryIdAsync(businessId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(
                new CreateProductCommand(businessId, Guid.NewGuid(), "Burger", null, 1m, true, userId),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenProductNameAlreadyExistsInCategory()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _categoryRepositoryMock
            .Setup(r => r.GetByBusinessIdAndCategoryIdAsync(businessId, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category { Id = categoryId, BusinessId = businessId });
        _productRepositoryMock
            .Setup(r => r.ExistsByNameInCategoryAsync(businessId, categoryId, "Burger", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _handler.Handle(
                new CreateProductCommand(businessId, categoryId, "Burger", null, 1m, true, userId),
                CancellationToken.None));

        _productRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
