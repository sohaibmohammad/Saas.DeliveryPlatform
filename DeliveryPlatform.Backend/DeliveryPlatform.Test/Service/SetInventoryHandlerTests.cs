using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Features.Inventory.SetInventory;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class SetInventoryHandlerTests
{
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly Mock<IBranchRepository> _branchRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SetInventoryHandler _handler;

    public SetInventoryHandlerTests()
    {
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _branchRepositoryMock = new Mock<IBranchRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new SetInventoryHandler(
            _inventoryRepositoryMock.Object,
            _businessRepositoryMock.Object,
            _branchRepositoryMock.Object,
            _productRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateInventory_WhenItDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        Inventory? createdInventory = null;

        SetupValidOwnership(userId, businessId, branchId, productId);
        _inventoryRepositoryMock
            .Setup(r => r.GetByBranchIdAndProductIdAsync(branchId, productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Inventory?)null);
        _inventoryRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Inventory>(), It.IsAny<CancellationToken>()))
            .Callback<Inventory, CancellationToken>((inventory, _) => createdInventory = inventory)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(
            new SetInventoryCommand(userId, businessId, branchId, productId, 15),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(createdInventory);
        Assert.Equal(result, createdInventory!.Id);
        Assert.Equal(branchId, createdInventory.BranchId);
        Assert.Equal(productId, createdInventory.ProductId);
        Assert.Equal(15, createdInventory.Quantity);
        Assert.Equal(0, createdInventory.ReservedQuantity);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateQuantity_WhenInventoryExists()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var inventory = new Inventory
        {
            Id = Guid.NewGuid(),
            BranchId = branchId,
            ProductId = productId,
            Quantity = 5,
            ReservedQuantity = 2
        };

        SetupValidOwnership(userId, businessId, branchId, productId);
        _inventoryRepositoryMock
            .Setup(r => r.GetByBranchIdAndProductIdAsync(branchId, productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);
        _inventoryRepositoryMock
            .Setup(r => r.SetQuantityAsync(inventory.Id, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(
            new SetInventoryCommand(userId, businessId, branchId, productId, 20),
            CancellationToken.None);

        Assert.Equal(inventory.Id, result);
        _inventoryRepositoryMock.Verify(
            r => r.SetQuantityAsync(inventory.Id, 20, It.IsAny<CancellationToken>()),
            Times.Once);
        _inventoryRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Inventory>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenQuantityIsNegative()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(
                new SetInventoryCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), -1),
                CancellationToken.None));

        _businessRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenBusinessNotFound()
    {
        _businessRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(
                new SetInventoryCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserDoesNotOwnBusiness()
    {
        var businessId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByIdAsync(businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = Guid.NewGuid() });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(
                new SetInventoryCommand(Guid.NewGuid(), businessId, Guid.NewGuid(), Guid.NewGuid(), 1),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenBranchDoesNotBelongToBusiness()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByIdAsync(businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _branchRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Branch { Id = Guid.NewGuid(), BusinessId = Guid.NewGuid() });

        await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(
                new SetInventoryCommand(userId, businessId, Guid.NewGuid(), Guid.NewGuid(), 1),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenProductDoesNotBelongToBusiness()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var branchId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByIdAsync(businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _branchRepositoryMock
            .Setup(r => r.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Branch { Id = branchId, BusinessId = businessId });
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { Id = Guid.NewGuid(), BusinessId = Guid.NewGuid() });

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(
                new SetInventoryCommand(userId, businessId, branchId, Guid.NewGuid(), 1),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenQuantityCannotGoBelowReserved()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var inventory = new Inventory { Id = Guid.NewGuid(), BranchId = branchId, ProductId = productId };

        SetupValidOwnership(userId, businessId, branchId, productId);
        _inventoryRepositoryMock
            .Setup(r => r.GetByBranchIdAndProductIdAsync(branchId, productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);
        _inventoryRepositoryMock
            .Setup(r => r.SetQuantityAsync(inventory.Id, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(
                new SetInventoryCommand(userId, businessId, branchId, productId, 1),
                CancellationToken.None));
    }

    private void SetupValidOwnership(Guid userId, Guid businessId, Guid branchId, Guid productId)
    {
        _businessRepositoryMock
            .Setup(r => r.GetByIdAsync(businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _branchRepositoryMock
            .Setup(r => r.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Branch { Id = branchId, BusinessId = businessId });
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { Id = productId, BusinessId = businessId });
    }
}
