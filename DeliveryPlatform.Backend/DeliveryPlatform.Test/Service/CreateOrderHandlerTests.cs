using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Application.src.Features.Order.Create;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class CreateOrderHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly Mock<IBranchRepository> _branchRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _branchRepositoryMock = new Mock<IBranchRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _unitOfWorkMock
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> operation, CancellationToken ct) => operation(ct));

        _handler = new CreateOrderHandler(
            _businessRepositoryMock.Object,
            _branchRepositoryMock.Object,
            _productRepositoryMock.Object,
            _inventoryRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateOrderAndReserveStock_WhenValid()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            BusinessId = businessId,
            Name = "Burger",
            Price = 10m,
            IsAvailable = true
        };
        Order? createdOrder = null;

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        SetupBranchAndBusiness(branchId, businessId, isBranchActive: true, isBusinessActive: true);
        _productRepositoryMock
            .Setup(r => r.GetByIdsAsync(businessId, It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });
        _inventoryRepositoryMock
            .Setup(r => r.ReserveStockAsync(branchId, productId, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _orderRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => createdOrder = order)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(
            new CreateOrderCommand(branchId, " 123 Main St ", new List<CreateOrderItemRequest>
            {
                new(productId, 2)
            }),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.OrderId);
        Assert.False(string.IsNullOrWhiteSpace(result.OrderNumber));
        Assert.Equal(20m, result.Subtotal);
        Assert.Equal(0m, result.DeliveryFee);
        Assert.Equal(20m, result.Total);
        Assert.Equal(OrderStatus.Pending, result.Status);
        Assert.NotNull(createdOrder);
        Assert.Equal(userId, createdOrder!.UserId);
        Assert.Equal(businessId, createdOrder.BusinessId);
        Assert.Equal(branchId, createdOrder.BranchId);
        Assert.Equal("123 Main St", createdOrder.DeliveryAddress);
        var item = Assert.Single(createdOrder.Items);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal("Burger", item.ProductName);
        Assert.Equal(10m, item.UnitPrice);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(20m, item.TotalPrice);
    }

    [Fact]
    public async Task Handle_ShouldGroupDuplicateProducts_WhenBuildingOrder()
    {
        var businessId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, BusinessId = businessId, Name = "Burger", Price = 10m };
        Order? createdOrder = null;

        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.NewGuid());
        SetupBranchAndBusiness(branchId, businessId, true, true);
        _productRepositoryMock
            .Setup(r => r.GetByIdsAsync(businessId, It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });
        _inventoryRepositoryMock
            .Setup(r => r.ReserveStockAsync(branchId, productId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _orderRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => createdOrder = order)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(
            new CreateOrderCommand(branchId, "Address", new List<CreateOrderItemRequest>
            {
                new(productId, 2),
                new(productId, 3)
            }),
            CancellationToken.None);

        Assert.Equal(50m, result.Subtotal);
        Assert.NotNull(createdOrder);
        var item = Assert.Single(createdOrder!.Items);
        Assert.Equal(5, item.Quantity);
        Assert.Equal(50m, item.TotalPrice);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenItemsAreEmpty()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(
                new CreateOrderCommand(Guid.NewGuid(), "Address", new List<CreateOrderItemRequest>()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenDeliveryAddressIsEmpty()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(
                new CreateOrderCommand(Guid.NewGuid(), "   ", new List<CreateOrderItemRequest>
                {
                    new(Guid.NewGuid(), 1)
                }),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenItemIsInvalid()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(
                new CreateOrderCommand(Guid.NewGuid(), "Address", new List<CreateOrderItemRequest>
                {
                    new(Guid.NewGuid(), 0)
                }),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenBranchIsNotActive()
    {
        var branchId = Guid.NewGuid();
        var businessId = Guid.NewGuid();

        SetupBranchAndBusiness(branchId, businessId, isBranchActive: false, isBusinessActive: true);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(CreateValidCommand(branchId), CancellationToken.None));

        _productRepositoryMock.Verify(
            r => r.GetByIdsAsync(It.IsAny<Guid>(), It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenBusinessIsNotActive()
    {
        var branchId = Guid.NewGuid();
        var businessId = Guid.NewGuid();

        SetupBranchAndBusiness(branchId, businessId, isBranchActive: true, isBusinessActive: false);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(CreateValidCommand(branchId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenNotAllProductsAreFound()
    {
        var branchId = Guid.NewGuid();
        var businessId = Guid.NewGuid();

        SetupBranchAndBusiness(branchId, businessId, true, true);
        _productRepositoryMock
            .Setup(r => r.GetByIdsAsync(businessId, It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(CreateValidCommand(branchId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenProductIsUnavailable()
    {
        var branchId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = "Burger",
            Price = 10m,
            IsAvailable = false
        };

        SetupBranchAndBusiness(branchId, businessId, true, true);
        _productRepositoryMock
            .Setup(r => r.GetByIdsAsync(businessId, It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(
                new CreateOrderCommand(branchId, "Address", new List<CreateOrderItemRequest>
                {
                    new(product.Id, 1)
                }),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenStockCannotBeReserved()
    {
        var branchId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = "Burger",
            Price = 10m,
            IsAvailable = true
        };

        SetupBranchAndBusiness(branchId, businessId, true, true);
        _productRepositoryMock
            .Setup(r => r.GetByIdsAsync(businessId, It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });
        _inventoryRepositoryMock
            .Setup(r => r.ReserveStockAsync(branchId, product.Id, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(
                new CreateOrderCommand(branchId, "Address", new List<CreateOrderItemRequest>
                {
                    new(product.Id, 1)
                }),
                CancellationToken.None));

        _orderRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void SetupBranchAndBusiness(Guid branchId, Guid businessId, bool isBranchActive, bool isBusinessActive)
    {
        _branchRepositoryMock
            .Setup(r => r.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Branch { Id = branchId, BusinessId = businessId, IsActive = isBranchActive });
        _businessRepositoryMock
            .Setup(r => r.GetByIdAsync(businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, IsActive = isBusinessActive });
    }

    private static CreateOrderCommand CreateValidCommand(Guid branchId) => new(
        branchId,
        "Address",
        new List<CreateOrderItemRequest> { new(Guid.NewGuid(), 1) });
}
