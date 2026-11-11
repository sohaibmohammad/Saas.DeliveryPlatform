using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Exceptions;
using DeliveryPlatform.Application.src.Features.Product.Update;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class UpdateProductHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateProductHandler _handler;

    public UpdateProductHandlerTests()
    {
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new UpdateProductHandler(
            _businessRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _productRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateProduct_WhenValid()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            CategoryId = Guid.NewGuid(),
            Name = "Old",
            Description = "OldDesc",
            Price = 5m,
            IsAvailable = true
        };

        SetupOwnershipAndCategory(userId, businessId, categoryId);
        _productRepositoryMock
            .Setup(r => r.GetByBusinessIdAndProductIdAsync(businessId, product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _productRepositoryMock
            .Setup(r => r.ExistsByNameInCategoryAsync(businessId, categoryId, "New", product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(
            new UpdateProductCommand(businessId, product.Id, categoryId, "  New  ", "  Desc  ", 20m, false, userId),
            CancellationToken.None);

        Assert.Equal(product.Id, result);
        Assert.Equal(categoryId, product.CategoryId);
        Assert.Equal("New", product.Name);
        Assert.Equal("Desc", product.Description);
        Assert.Equal(20m, product.Price);
        Assert.False(product.IsAvailable);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldClearWhitespaceDescription()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            CategoryId = categoryId,
            Name = "Old",
            Description = "OldDesc"
        };

        SetupOwnershipAndCategory(userId, businessId, categoryId);
        _productRepositoryMock
            .Setup(r => r.GetByBusinessIdAndProductIdAsync(businessId, product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _productRepositoryMock
            .Setup(r => r.ExistsByNameInCategoryAsync(businessId, categoryId, It.IsAny<string>(), product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _handler.Handle(
            new UpdateProductCommand(businessId, product.Id, categoryId, "New", "   ", 1m, true, userId),
            CancellationToken.None);

        Assert.Null(product.Description);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenBusinessNotFound()
    {
        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(CreateCommand(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenProductNotFound()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        SetupOwnershipAndCategory(userId, businessId, categoryId);
        _productRepositoryMock
            .Setup(r => r.GetByBusinessIdAndProductIdAsync(businessId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(
                new UpdateProductCommand(businessId, Guid.NewGuid(), categoryId, "New", null, 1m, true, userId),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCategoryNotFound()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), BusinessId = businessId, CategoryId = Guid.NewGuid() };

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _productRepositoryMock
            .Setup(r => r.GetByBusinessIdAndProductIdAsync(businessId, product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _categoryRepositoryMock
            .Setup(r => r.GetByBusinessIdAndCategoryIdAsync(businessId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(
                new UpdateProductCommand(businessId, product.Id, Guid.NewGuid(), "New", null, 1m, true, userId),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenProductNameAlreadyExistsInCategory()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), BusinessId = businessId, CategoryId = categoryId, Name = "Old" };

        SetupOwnershipAndCategory(userId, businessId, categoryId);
        _productRepositoryMock
            .Setup(r => r.GetByBusinessIdAndProductIdAsync(businessId, product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _productRepositoryMock
            .Setup(r => r.ExistsByNameInCategoryAsync(businessId, categoryId, "New", product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _handler.Handle(
                new UpdateProductCommand(businessId, product.Id, categoryId, "New", null, 1m, true, userId),
                CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private void SetupOwnershipAndCategory(Guid userId, Guid businessId, Guid categoryId)
    {
        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _categoryRepositoryMock
            .Setup(r => r.GetByBusinessIdAndCategoryIdAsync(businessId, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category { Id = categoryId, BusinessId = businessId });
    }

    private static UpdateProductCommand CreateCommand() => new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        "New",
        null,
        1m,
        true,
        Guid.NewGuid());
}
