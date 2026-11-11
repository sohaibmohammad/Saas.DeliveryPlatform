using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Exceptions;
using DeliveryPlatform.Application.src.Features.Category.Create;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class CreateCategoryHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateCategoryHandler _handler;

    public CreateCategoryHandlerTests()
    {
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateCategoryHandler(
            _businessRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateCategory_WhenValid()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        Category? createdCategory = null;

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _categoryRepositoryMock
            .Setup(r => r.GetByBusinessIdAndNameAsync(businessId, "Drinks", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);
        _categoryRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .Callback<Category, CancellationToken>((category, _) => createdCategory = category)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(
            new CreateCategoryCommand(businessId, "  Drinks  ", userId),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(createdCategory);
        Assert.Equal(result, createdCategory!.Id);
        Assert.Equal(businessId, createdCategory.BusinessId);
        Assert.Equal("Drinks", createdCategory.Name);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenBusinessNotFound()
    {
        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new CreateCategoryCommand(Guid.NewGuid(), "Drinks", Guid.NewGuid()), CancellationToken.None));

        _categoryRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCategoryNameAlreadyExists()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _categoryRepositoryMock
            .Setup(r => r.GetByBusinessIdAndNameAsync(businessId, "Drinks", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category { Id = Guid.NewGuid(), BusinessId = businessId, Name = "Drinks" });

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _handler.Handle(new CreateCategoryCommand(businessId, "Drinks", userId), CancellationToken.None));

        _categoryRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
