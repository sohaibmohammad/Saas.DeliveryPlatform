using DeliveryPlatform.Application.src.Abstractions.Notifications;
using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Application.src.Features.Order.UpdateStatus;
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
    private readonly Mock<IOrderNotificationService> _notificationServiceMock;
    private readonly UpdateOrderStatusHandler _handler;

    public UpdateOrderStatusHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _notificationServiceMock = new Mock<IOrderNotificationService>();

        _unitOfWorkMock
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> operation, CancellationToken ct) => operation(ct));
        _orderRepositoryMock
            .Setup(r => r.TryUpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<OrderStatus>(), It.IsAny<OrderStatus>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _notificationServiceMock
            .Setup(n => n.NotifyOrderStatusChangedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _handler = new UpdateOrderStatusHandler(
            _notificationServiceMock.Object,
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
            .Setup(r => r.GetOrderForStatusUpdateAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(
            new UpdateOrderStatusCommand(order.Id, OrderStatus.Confirmed),
            CancellationToken.None);

        Assert.True(result);
        _orderRepositoryMock.Verify(
            r => r.TryUpdateStatusAsync(order.Id, OrderStatus.Pending, OrderStatus.Confirmed, It.IsAny<CancellationToken>()),
            Times.Once);
        _orderRepositoryMock.Verify(
            r => r.AddStatusHistory(It.Is<OrderStatusHistory>(h =>
                h.OrderId == order.Id &&
                h.PreviousStatus == OrderStatus.Pending &&
                h.NewStatus == OrderStatus.Confirmed &&
                h.ChangedBy == userId)),
            Times.Once);
        _notificationServiceMock.Verify(
            n => n.NotifyOrderStatusChangedAsync(order.UserId, order.Id, "Confirmed"),
            Times.Once);
        _inventoryRepositoryMock.Verify(
            r => r.CommitReservedStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
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
            .Setup(r => r.GetOrderForStatusUpdateAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _inventoryRepositoryMock
            .Setup(r => r.CommitReservedStockAsync(order.BranchId, It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(
            new UpdateOrderStatusCommand(order.Id, OrderStatus.Delivered),
            CancellationToken.None);

        Assert.True(result);
        foreach (var item in order.Items)
        {
            _inventoryRepositoryMock.Verify(
                r => r.CommitReservedStockAsync(order.BranchId, item.ProductId, item.Quantity, It.IsAny<CancellationToken>()),
                Times.Once);
        }
        _unitOfWorkMock.Verify(
            u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _notificationServiceMock.Verify(
            n => n.NotifyOrderStatusChangedAsync(order.UserId, order.Id, "Delivered"),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenOrderIdIsEmpty()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new UpdateOrderStatusCommand(Guid.Empty, OrderStatus.Confirmed), CancellationToken.None));

        _orderRepositoryMock.Verify(
            r => r.GetOrderForStatusUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _notificationServiceMock.Verify(
            n => n.NotifyOrderStatusChangedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenOrderNotFound()
    {
        _orderRepositoryMock
            .Setup(r => r.GetOrderForStatusUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(new UpdateOrderStatusCommand(Guid.NewGuid(), OrderStatus.Confirmed), CancellationToken.None));

        _notificationServiceMock.Verify(
            n => n.NotifyOrderStatusChangedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserDoesNotOwnBusiness()
    {
        var order = CreateOrder(Guid.NewGuid(), OrderStatus.Pending);

        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.NewGuid());
        _orderRepositoryMock
            .Setup(r => r.GetOrderForStatusUpdateAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(new UpdateOrderStatusCommand(order.Id, OrderStatus.Confirmed), CancellationToken.None));

        _orderRepositoryMock.Verify(
            r => r.TryUpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<OrderStatus>(), It.IsAny<OrderStatus>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _notificationServiceMock.Verify(
            n => n.NotifyOrderStatusChangedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never);
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
            .Setup(r => r.GetOrderForStatusUpdateAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new UpdateOrderStatusCommand(order.Id, next), CancellationToken.None));

        _orderRepositoryMock.Verify(
            r => r.TryUpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<OrderStatus>(), It.IsAny<OrderStatus>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _notificationServiceMock.Verify(
            n => n.NotifyOrderStatusChangedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never);
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
            .Setup(r => r.GetOrderForStatusUpdateAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _inventoryRepositoryMock
            .Setup(r => r.CommitReservedStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new UpdateOrderStatusCommand(order.Id, OrderStatus.Delivered), CancellationToken.None));

        _orderRepositoryMock.Verify(
            r => r.AddStatusHistory(It.IsAny<OrderStatusHistory>()),
            Times.Never);
        _notificationServiceMock.Verify(
            n => n.NotifyOrderStatusChangedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never);
    }

    [Theory]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task Handle_ShouldThrow_WhenOrderIsInTerminalStatus(OrderStatus current)
    {
        var userId = Guid.NewGuid();
        var order = CreateOrder(userId, current);

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _orderRepositoryMock
            .Setup(r => r.GetOrderForStatusUpdateAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new UpdateOrderStatusCommand(order.Id, OrderStatus.Confirmed), CancellationToken.None));

        _inventoryRepositoryMock.Verify(
            r => r.CommitReservedStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _notificationServiceMock.Verify(
            n => n.NotifyOrderStatusChangedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenStatusWasChangedConcurrently()
    {
        var userId = Guid.NewGuid();
        var order = CreateOrder(userId, OrderStatus.Pending);

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _orderRepositoryMock
            .Setup(r => r.GetOrderForStatusUpdateAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _orderRepositoryMock
            .Setup(r => r.TryUpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<OrderStatus>(), It.IsAny<OrderStatus>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new UpdateOrderStatusCommand(order.Id, OrderStatus.Confirmed), CancellationToken.None));

        _orderRepositoryMock.Verify(
            r => r.AddStatusHistory(It.IsAny<OrderStatusHistory>()),
            Times.Never);
        _notificationServiceMock.Verify(
            n => n.NotifyOrderStatusChangedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never);
    }

    private static Order CreateOrder(Guid businessOwnerId, OrderStatus status) => new()
    {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        BranchId = Guid.NewGuid(),
        Status = status,
        UpdatedAt = DateTime.UtcNow.AddHours(-1),
        Business = new Business { Id = Guid.NewGuid(), UserId = businessOwnerId },
        Items = new List<OrderItem>()
    };
}
