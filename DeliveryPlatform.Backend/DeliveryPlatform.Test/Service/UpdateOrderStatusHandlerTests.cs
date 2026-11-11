using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Application.src.Features.Order.UpdateStaus;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class UpdateOrderStatusHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateOrderStatusHandler _handler;

    public UpdateOrderStatusHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _unitOfWorkMock
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> operation, CancellationToken ct) => operation(ct));

        _handler = new UpdateOrderStatusHandler(
            _inventoryRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateStatus_ForValidTransition()
    {
        var userId = Guid.NewGuid();
        var order = CreateOrder(userId, OrderStatus.Pending);

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _orderRepositoryMock
            .Setup(r => r.GetOrderForSatusUpdateAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(
            new UpdaeOrderStatusCommand(order.Id, OrderStatus.Confirmed),
            CancellationToken.None);

        Assert.True(result);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
        Assert.True(order.UpdatedAt > DateTime.UtcNow.AddMinutes(-1));
        _inventoryRepositoryMock.Verify(
            r => r.CommitReservedStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCommitReservedStock_WhenOrderIsDelivered()
    {
        var userId = Guid.NewGuid();
        var order = CreateOrder(userId, OrderStatus.OutForDelivery);
        order.Items = new List<OrderItem>
        {
            new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 2 },
            new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 3 }
        };

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _orderRepositoryMock
            .Setup(r => r.GetOrderForSatusUpdateAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _inventoryRepositoryMock
            .Setup(r => r.CommitReservedStockAsync(order.BranchId, It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(
            new UpdaeOrderStatusCommand(order.Id, OrderStatus.Delivered),
            CancellationToken.None);

        Assert.True(result);
        Assert.Equal(OrderStatus.Delivered, order.Status);
        foreach (var item in order.Items)
        {
            _inventoryRepositoryMock.Verify(
                r => r.CommitReservedStockAsync(order.BranchId, item.ProductId, item.Quantity, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenOrderIdIsEmpty()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new UpdaeOrderStatusCommand(Guid.Empty, OrderStatus.Confirmed), CancellationToken.None));

        _orderRepositoryMock.Verify(
            r => r.GetOrderForSatusUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenOrderNotFound()
    {
        _orderRepositoryMock
            .Setup(r => r.GetOrderForSatusUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(new UpdaeOrderStatusCommand(Guid.NewGuid(), OrderStatus.Confirmed), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserDoesNotOwnBusiness()
    {
        var order = CreateOrder(Guid.NewGuid(), OrderStatus.Pending);

        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.NewGuid());
        _orderRepositoryMock
            .Setup(r => r.GetOrderForSatusUpdateAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(new UpdaeOrderStatusCommand(order.Id, OrderStatus.Confirmed), CancellationToken.None));

        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Pending)]
    [InlineData(OrderStatus.Delivered, OrderStatus.OutForDelivery)]
    public async Task Handle_ShouldThrow_ForInvalidTransition(OrderStatus current, OrderStatus next)
    {
        var userId = Guid.NewGuid();
        var order = CreateOrder(userId, current);

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _orderRepositoryMock
            .Setup(r => r.GetOrderForSatusUpdateAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new UpdaeOrderStatusCommand(order.Id, next), CancellationToken.None));

        Assert.Equal(current, order.Status);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenStockCommitFails()
    {
        var userId = Guid.NewGuid();
        var order = CreateOrder(userId, OrderStatus.OutForDelivery);
        order.Items = new List<OrderItem>
        {
            new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 1 }
        };

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _orderRepositoryMock
            .Setup(r => r.GetOrderForSatusUpdateAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _inventoryRepositoryMock
            .Setup(r => r.CommitReservedStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new UpdaeOrderStatusCommand(order.Id, OrderStatus.Delivered), CancellationToken.None));

        Assert.Equal(OrderStatus.OutForDelivery, order.Status);
    }

    private static Order CreateOrder(Guid businessOwnerId, OrderStatus status) => new()
    {
        Id = Guid.NewGuid(),
        BranchId = Guid.NewGuid(),
        Status = status,
        UpdatedAt = DateTime.UtcNow.AddHours(-1),
        Business = new Business { Id = Guid.NewGuid(), UserId = businessOwnerId },
        Items = new List<OrderItem>()
    };
}
