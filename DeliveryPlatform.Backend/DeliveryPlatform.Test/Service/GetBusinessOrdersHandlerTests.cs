using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Application.src.Common.Paging;
using DeliveryPlatform.Application.src.Features.Order.GetBusinessOrders;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class GetBusinessOrdersHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly Mock<IBranchRepository> _branchRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetBusinessOrdersHandler _handler;

    public GetBusinessOrdersHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _branchRepositoryMock = new Mock<IBranchRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new GetBusinessOrdersHandler(
            _orderRepositoryMock.Object,
            _businessRepositoryMock.Object,
            _branchRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrdersFromAllBranches_WhenUserOwnsBusiness()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var order = CreateOrder(businessId, Guid.NewGuid());

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId, Name = "Shop" });
        _orderRepositoryMock
            .Setup(r => r.GetByBusinessIdPagedAsync(
                businessId,
                null,
                null,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Order>
            {
                Items = new List<Order> { order },
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 1
            });

        var result = await _handler.Handle(
            new GetBusinessOrdersQuery(businessId, 1, 10, null),
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(order.Id, result.Items[0].OrderId);
        Assert.Equal(order.OrderNumber, result.Items[0].OrderNumber);
        _branchRepositoryMock.Verify(
            r => r.GetByBusinessIdAndBranchIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFilterByBranch_WhenBranchBelongsToBusiness()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var order = CreateOrder(businessId, branchId);

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId, Name = "Shop" });
        _branchRepositoryMock
            .Setup(r => r.GetByBusinessIdAndBranchIdAsync(businessId, branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Branch { Id = branchId, BusinessId = businessId, Name = "Branch", Address = "Address" });
        _orderRepositoryMock
            .Setup(r => r.GetByBusinessIdPagedAsync(
                businessId,
                branchId,
                OrderStatus.Delivered,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Order>
            {
                Items = new List<Order> { order },
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 1
            });

        var result = await _handler.Handle(
            new GetBusinessOrdersQuery(businessId, 1, 10, OrderStatus.Delivered, branchId),
            CancellationToken.None);

        Assert.Single(result.Items);
        _orderRepositoryMock.Verify(
            r => r.GetByBusinessIdPagedAsync(
                businessId,
                branchId,
                OrderStatus.Delivered,
                1,
                10,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserDoesNotOwnBusiness()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(
                new GetBusinessOrdersQuery(businessId, 1, 10, null),
                CancellationToken.None));

        _orderRepositoryMock.Verify(
            r => r.GetByBusinessIdPagedAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<OrderStatus?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenBranchDoesNotBelongToBusiness()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var branchId = Guid.NewGuid();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId, Name = "Shop" });
        _branchRepositoryMock
            .Setup(r => r.GetByBusinessIdAndBranchIdAsync(businessId, branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(
                new GetBusinessOrdersQuery(businessId, 1, 10, null, branchId),
                CancellationToken.None));

        _orderRepositoryMock.Verify(
            r => r.GetByBusinessIdPagedAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<OrderStatus?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Order CreateOrder(Guid businessId, Guid branchId)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            BusinessId = businessId,
            BranchId = branchId,
            OrderNumber = "ORD-1001",
            Status = OrderStatus.Delivered,
            Subtotal = 50m,
            DeliveryFee = 5m,
            Total = 55m,
            DeliveryAddress = "Test Address",
            CreatedAt = DateTime.UtcNow
        };
    }
}
