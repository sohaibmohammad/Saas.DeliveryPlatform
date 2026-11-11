using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Application.src.Common.Paging;
using DeliveryPlatform.Application.src.Features.Order.Get;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class GetMyOrdersHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetMyOrdersHandler _handler;

    public GetMyOrdersHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new GetMyOrdersHandler(_orderRepositoryMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedPagedResult()
    {
        var userId = Guid.NewGuid();
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-1",
            Status = OrderStatus.Pending,
            Subtotal = 20m,
            DeliveryFee = 5m,
            Total = 25m,
            DeliveryAddress = "Address",
            CreatedAt = DateTime.UtcNow
        };

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _orderRepositoryMock
            .Setup(r => r.GetByUserIdPagedAsync(userId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Order>
            {
                Items = new List<Order> { order },
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 1
            });

        var result = await _handler.Handle(new GetMyOrdersQuery(1, 10), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(order.Id, result.Items[0].OrderId);
        Assert.Equal("ORD-1", result.Items[0].OrderNumber);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task Handle_ShouldClampPaging_WhenOutOfRange()
    {
        var userId = Guid.NewGuid();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _orderRepositoryMock
            .Setup(r => r.GetByUserIdPagedAsync(userId, 1, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Order>
            {
                Items = new List<Order>(),
                PageNumber = 1,
                PageSize = 50,
                TotalCount = 0
            });

        var result = await _handler.Handle(new GetMyOrdersQuery(0, 100), CancellationToken.None);

        Assert.Empty(result.Items);
        _orderRepositoryMock.Verify(
            r => r.GetByUserIdPagedAsync(userId, 1, 50, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
