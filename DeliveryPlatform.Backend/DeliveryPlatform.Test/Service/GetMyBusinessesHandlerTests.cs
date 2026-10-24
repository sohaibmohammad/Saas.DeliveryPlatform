using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Features.Business.GetMy;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class GetMyBusinessesHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly GetMyBusinessesHandler _handler;

    public GetMyBusinessesHandlerTests()
    {
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _handler = new GetMyBusinessesHandler(_businessRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResult_WhenBusinessesExist()
    {
        var userId = Guid.NewGuid();
        var businesses = new List<Business>
        {
            new Business { Id = Guid.NewGuid(), UserId = userId, Name = "Shop1", BusinessType = BusinessType.Restaurant, CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
            new Business { Id = Guid.NewGuid(), UserId = userId, Name = "Shop2", BusinessType = BusinessType.Grocery, CreatedAt = DateTime.UtcNow }
        };

        _businessRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(businesses);

        var result = await _handler.Handle(new GetMyBusinessesQuery(userId, 1, 10), CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
    }
}
