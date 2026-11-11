using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Application.src.Features.Order.Cancel;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class CancelOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CancelOrderHandler _handler;

    public CancelOrderHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _unitOfWorkMock
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> operation, CancellationToken ct) => operation(ct));

        _handler = new CancelOrderHandler(
            _inventoryRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCancelOrderAndReleaseStock_WhenOrderIsPending()
    {
        var userId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BranchId = branchId,
            Status = OrderStatus.Pending,
            UpdatedAt = DateTime.UtcNow.AddHours(-1),
            Items = new List<OrderItem>
            {
                new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 2 },
                new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 3 }
            }
        };

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _orderRepositoryMock
            .Setup(r => r.GetForCancellationAsync(order.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _inventoryRepositoryMock
            .Setup(r => r.ReleaseReservedStockAsync(branchId, It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None);

        Assert.True(result);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.True(order.UpdatedAt > DateTime.UtcNow.AddMinutes(-1));
        foreach (var item in order.Items)
        {
            _inventoryRepositoryMock.Verify(
                r => r.ReleaseReservedStockAsync(branchId, item.ProductId, item.Quantity, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenOrderIdIsEmpty()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new CancelOrderCommand(Guid.Empty), CancellationToken.None));

        _orderRepositoryMock.Verify(
            r => r.GetForCancellationAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenOrderNotFound()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.NewGuid());
        _orderRepositoryMock
            .Setup(r => r.GetForCancellationAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(new CancelOrderCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Theory]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task Handle_ShouldThrow_WhenOrderIsNotPending(OrderStatus status)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Status = status,
            Items = new List<OrderItem>()
        };

        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.NewGuid());
        _orderRepositoryMock
            .Setup(r => r.GetForCancellationAsync(order.Id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None));

        _inventoryRepositoryMock.Verify(
            r => r.ReleaseReservedStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenStockReleaseFails()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            Items = new List<OrderItem>
            {
                new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.NewGuid());
        _orderRepositoryMock
            .Setup(r => r.GetForCancellationAsync(order.Id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _inventoryRepositoryMock
            .Setup(r => r.ReleaseReservedStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None));

        Assert.Equal(OrderStatus.Pending, order.Status);
    }
}
