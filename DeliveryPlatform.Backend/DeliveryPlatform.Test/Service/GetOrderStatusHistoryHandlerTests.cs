using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Application.src.Features.Order.GetStatusHistory;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class GetOrderStatusHistoryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetOrderStatusHistoryHandler _handler;

    public GetOrderStatusHistoryHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new GetOrderStatusHistoryHandler(
            _orderRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnHistoryOrderedByChangedAt()
    {
        var userId = Guid.NewGuid();
        var order = CreateOrder(userId);
        var firstChangedAt = DateTime.UtcNow.AddHours(-3);
        var secondChangedAt = DateTime.UtcNow.AddHours(-2);
        var thirdChangedAt = DateTime.UtcNow.AddHours(-1);
        var firstChangedBy = Guid.NewGuid();
        var secondChangedBy = Guid.NewGuid();
        var thirdChangedBy = Guid.NewGuid();

        order.StatusHistory = new List<OrderStatusHistory>
        {
            new()
            {
                OrderId = order.Id,
                PreviousStatus = OrderStatus.Confirmed,
                NewStatus = OrderStatus.Preparing,
                ChangedBy = secondChangedBy,
                ChangedAt = secondChangedAt
            },
            new()
            {
                OrderId = order.Id,
                PreviousStatus = null,
                NewStatus = OrderStatus.Confirmed,
                ChangedBy = firstChangedBy,
                ChangedAt = firstChangedAt
            },
            new()
            {
                OrderId = order.Id,
                PreviousStatus = OrderStatus.Preparing,
                NewStatus = OrderStatus.ReadyForPickup,
                ChangedBy = thirdChangedBy,
                ChangedAt = thirdChangedAt
            }
        };

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _orderRepositoryMock
            .Setup(r => r.GetOrderWithStatusHistoryForUserAsync(order.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new GetOrderStatusHistoryQuery(order.Id), CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal(firstChangedAt, result[0].ChangedAt);
        Assert.Equal(secondChangedAt, result[1].ChangedAt);
        Assert.Equal(thirdChangedAt, result[2].ChangedAt);
        Assert.Null(result[0].PreviousStatus);
        Assert.Equal(OrderStatus.Confirmed, result[0].NewStatus);
        Assert.Equal(firstChangedBy, result[0].ChangedBy);
        Assert.Equal(OrderStatus.Confirmed, result[1].PreviousStatus);
        Assert.Equal(OrderStatus.Preparing, result[1].NewStatus);
        Assert.Equal(secondChangedBy, result[1].ChangedBy);
        Assert.Equal(OrderStatus.Preparing, result[2].PreviousStatus);
        Assert.Equal(OrderStatus.ReadyForPickup, result[2].NewStatus);
        Assert.Equal(thirdChangedBy, result[2].ChangedBy);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenOrderHasNoHistory()
    {
        var userId = Guid.NewGuid();
        var order = CreateOrder(userId);

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _orderRepositoryMock
            .Setup(r => r.GetOrderWithStatusHistoryForUserAsync(order.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new GetOrderStatusHistoryQuery(order.Id), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ShouldQueryUsingCurrentUserId()
    {
        var userId = Guid.NewGuid();
        var order = CreateOrder(userId);

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _orderRepositoryMock
            .Setup(r => r.GetOrderWithStatusHistoryForUserAsync(order.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await _handler.Handle(new GetOrderStatusHistoryQuery(order.Id), CancellationToken.None);

        _orderRepositoryMock.Verify(
            r => r.GetOrderWithStatusHistoryForUserAsync(order.Id, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenOrderIdIsEmpty()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new GetOrderStatusHistoryQuery(Guid.Empty), CancellationToken.None));

        _orderRepositoryMock.Verify(
            r => r.GetOrderWithStatusHistoryForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenOrderNotFound()
    {
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _orderRepositoryMock
            .Setup(r => r.GetOrderWithStatusHistoryForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(new GetOrderStatusHistoryQuery(Guid.NewGuid()), CancellationToken.None));
    }

    private static Order CreateOrder(Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        BusinessId = Guid.NewGuid(),
        BranchId = Guid.NewGuid(),
        OrderNumber = "ORD-1",
        Status = OrderStatus.Confirmed,
        StatusHistory = new List<OrderStatusHistory>()
    };
}
