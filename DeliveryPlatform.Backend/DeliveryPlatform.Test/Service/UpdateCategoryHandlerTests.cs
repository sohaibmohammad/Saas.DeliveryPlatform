using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Exceptions;
using DeliveryPlatform.Application.src.Features.Category.Update;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class UpdateCategoryHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateCategoryHandler _handler;

    public UpdateCategoryHandlerTests()
    {
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateCategoryHandler(
            _businessRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateCategory_WhenValid()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, BusinessId = businessId, Name = "Old" };

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _categoryRepositoryMock
            .Setup(r => r.GetByBusinessIdAndCategoryIdAsync(businessId, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepositoryMock
            .Setup(r => r.GetByBusinessIdAndNameAsync(businessId, "New", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(
            new UpdateCategoryCommand(businessId, categoryId, "  New  ", userId),
            CancellationToken.None);

        Assert.Equal(categoryId, result);
        Assert.Equal("New", category.Name);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAllowSameName_WhenItBelongsToTheSameCategory()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, BusinessId = businessId, Name = "Drinks" };

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _categoryRepositoryMock
            .Setup(r => r.GetByBusinessIdAndCategoryIdAsync(businessId, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepositoryMock
            .Setup(r => r.GetByBusinessIdAndNameAsync(businessId, "Drinks", It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(
            new UpdateCategoryCommand(businessId, categoryId, "Drinks", userId),
            CancellationToken.None);

        Assert.Equal(categoryId, result);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenBusinessNotFound()
    {
        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(
                new UpdateCategoryCommand(Guid.NewGuid(), Guid.NewGuid(), "New", Guid.NewGuid()),
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
                new UpdateCategoryCommand(businessId, Guid.NewGuid(), "New", userId),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenAnotherCategoryHasTheSameName()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, BusinessId = businessId, Name = "Old" };

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _categoryRepositoryMock
            .Setup(r => r.GetByBusinessIdAndCategoryIdAsync(businessId, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepositoryMock
            .Setup(r => r.GetByBusinessIdAndNameAsync(businessId, "New", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category { Id = Guid.NewGuid(), BusinessId = businessId, Name = "New" });

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _handler.Handle(
                new UpdateCategoryCommand(businessId, categoryId, "New", userId),
                CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
