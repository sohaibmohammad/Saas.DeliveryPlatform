using AutoMapper;
using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Exceptions;
using DeliveryPlatform.Application.src.Features.Category.GetById;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class GetCategoryByIdHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetCategoryByIdHandler _handler;

    public GetCategoryByIdHandlerTests()
    {
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapperMock = new Mock<IMapper>();

        _mapperMock
            .Setup(m => m.Map<CategoryByIdResponse>(It.IsAny<Category>()))
            .Returns((Category category) => new CategoryByIdResponse(category.Id, category.BusinessId, category.Name));

        _handler = new GetCategoryByIdHandler(
            _businessRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCategory_WhenFound()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _categoryRepositoryMock
            .Setup(r => r.GetByBusinessIdAndCategoryIdAsync(businessId, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category { Id = categoryId, BusinessId = businessId, Name = "Drinks" });

        var result = await _handler.Handle(
            new GetCategoryByIdQuery(businessId, categoryId, userId, false),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(categoryId, result!.Id);
        Assert.Equal("Drinks", result.Name);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenCategoryNotFound()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _categoryRepositoryMock
            .Setup(r => r.GetByBusinessIdAndCategoryIdAsync(businessId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await _handler.Handle(
            new GetCategoryByIdQuery(businessId, Guid.NewGuid(), userId, false),
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
                new GetCategoryByIdQuery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), false),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldSkipOwnershipCheck_WhenIsAdmin()
    {
        var businessId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        _categoryRepositoryMock
            .Setup(r => r.GetByBusinessIdAndCategoryIdAsync(businessId, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Category { Id = categoryId, BusinessId = businessId, Name = "Drinks" });

        var result = await _handler.Handle(
            new GetCategoryByIdQuery(businessId, categoryId, Guid.NewGuid(), true),
            CancellationToken.None);

        Assert.NotNull(result);
        _businessRepositoryMock.Verify(
            r => r.GetByUserIdAndBusinessIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
