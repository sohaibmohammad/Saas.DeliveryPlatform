using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Repository;

public class BusinessRepositoryTests
{
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;

    public BusinessRepositoryTests()
    {
        _businessRepositoryMock = new Mock<IBusinessRepository>();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnBusinesses_ForGivenUser()
    {
        var userId = Guid.NewGuid();
        var businesses = new List<Business>
        {
            new Business { Id = Guid.NewGuid(), UserId = userId, Name = "Shop1", BusinessType = BusinessType.Restaurant },
            new Business { Id = Guid.NewGuid(), UserId = userId, Name = "Shop2", BusinessType = BusinessType.Grocery }
        };

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(businesses);

        var result = await _businessRepositoryMock.Object.GetByUserIdAsync(userId, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result, b => Assert.Equal(userId, b.UserId));
    }

    [Fact]
    public async Task GetByUserIdAndBusinessIdAsync_ShouldReturnBusiness_WhenExists()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var business = new Business { Id = businessId, UserId = userId, Name = "Shop1", BusinessType = BusinessType.Restaurant };

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);

        var result = await _businessRepositoryMock.Object.GetByUserIdAndBusinessIdAsync(userId, businessId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(businessId, result!.Id);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task AddAsync_ShouldCallRepository_WhenCalled()
    {
        var business = new Business { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "NewShop", BusinessType = BusinessType.Other };
        _businessRepositoryMock.Setup(r => r.AddAsync(business, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _businessRepositoryMock.Object.AddAsync(business, CancellationToken.None);

        _businessRepositoryMock.Verify(r => r.AddAsync(business, It.IsAny<CancellationToken>()), Times.Once);
    }
}
