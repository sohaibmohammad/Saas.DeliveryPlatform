using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Application.src.Features.Order.GetDetails;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class GetOrderDetailsHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetOrderDetailsHandler _handler;

    public GetOrderDetailsHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new GetOrderDetailsHandler(_orderRepositoryMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrderDetails_WhenOrderExists()
    {
        var userId = Guid.NewGuid();
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-1",
            Status = OrderStatus.Confirmed,
            DeliveryAddress = "Address",
            Subtotal = 20m,
            DeliveryFee = 5m,
            Total = 25m,
            CreatedAt = DateTime.UtcNow,
            Business = new Business { Id = Guid.NewGuid(), Name = "Shop" },
            Branch = new Branch { Id = Guid.NewGuid(), Name = "Main" },
            Items = new List<OrderItem>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Burger",
                    UnitPrice = 10m,
                    Quantity = 2,
                    TotalPrice = 20m
                }
            }
        };

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _orderRepositoryMock
            .Setup(r => r.GetOrderDetailsAsync(order.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new GetOrderDetailsQuery(order.Id), CancellationToken.None);

        Assert.Equal(order.Id, result.OrderId);
        Assert.Equal("ORD-1", result.OrderNumber);
        Assert.Equal("Shop", result.BusinessName);
        Assert.Equal("Main", result.BranchName);
        Assert.Equal(25m, result.Total);
        var item = Assert.Single(result.Items);
        Assert.Equal("Burger", item.ProductName);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(20m, item.TotalPrice);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenOrderIdIsEmpty()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new GetOrderDetailsQuery(Guid.Empty), CancellationToken.None));

        _orderRepositoryMock.Verify(
            r => r.GetOrderDetailsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenOrderNotFound()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.NewGuid());
        _orderRepositoryMock
            .Setup(r => r.GetOrderDetailsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new GetOrderDetailsQuery(Guid.NewGuid()), CancellationToken.None));
    }
}
