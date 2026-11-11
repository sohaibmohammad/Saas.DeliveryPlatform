using AutoMapper;
using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Exceptions;
using DeliveryPlatform.Application.src.Features.Category.GetByBusiness;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class GetCategoriesHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetCategoriesHandler _handler;

    public GetCategoriesHandlerTests()
    {
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapperMock = new Mock<IMapper>();

        _mapperMock
            .Setup(m => m.Map<CategoryResponse>(It.IsAny<Category>()))
            .Returns((Category category) => new CategoryResponse(category.Id, category.BusinessId, category.Name));

        _handler = new GetCategoriesHandler(
            _businessRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedCategories_WhenUserOwnsBusiness()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _categoryRepositoryMock
            .Setup(r => r.GetByBusinessIdAsync(businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>
            {
                CreateCategory(businessId, "A"),
                CreateCategory(businessId, "B"),
                CreateCategory(businessId, "C")
            });

        var result = await _handler.Handle(
            new GetCategoriesQuery(businessId, userId, false, 1, 2),
            CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal("A", result.Items[0].Name);
        Assert.Equal("B", result.Items[1].Name);
    }

    [Fact]
    public async Task Handle_ShouldSkipOwnershipCheck_WhenIsAdmin()
    {
        var businessId = Guid.NewGuid();

        _categoryRepositoryMock
            .Setup(r => r.GetByBusinessIdAsync(businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        await _handler.Handle(
            new GetCategoriesQuery(businessId, Guid.NewGuid(), true),
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
            () => _handler.Handle(new GetCategoriesQuery(Guid.NewGuid(), Guid.NewGuid(), false), CancellationToken.None));

        _categoryRepositoryMock.Verify(
            r => r.GetByBusinessIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Category CreateCategory(Guid businessId, string name) => new()
    {
        Id = Guid.NewGuid(),
        BusinessId = businessId,
        Name = name
    };
}
