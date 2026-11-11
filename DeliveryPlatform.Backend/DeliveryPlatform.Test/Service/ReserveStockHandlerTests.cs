using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Features.Inventory.ReserveStock;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class ReserveStockHandlerTests
{
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly ReserveStockHandler _handler;

    public ReserveStockHandlerTests()
    {
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _handler = new ReserveStockHandler(_inventoryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTrue_WhenStockIsReserved()
    {
        var branchId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _inventoryRepositoryMock
            .Setup(r => r.ReserveStockAsync(branchId, productId, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(
            new ReserveStockCommand(branchId, productId, 2),
            CancellationToken.None);

        Assert.True(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_ShouldThrow_WhenQuantityIsNotPositive(int quantity)
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(new ReserveStockCommand(Guid.NewGuid(), Guid.NewGuid(), quantity), CancellationToken.None));

        _inventoryRepositoryMock.Verify(
            r => r.ReserveStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenStockIsInsufficient()
    {
        _inventoryRepositoryMock
            .Setup(r => r.ReserveStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new ReserveStockCommand(Guid.NewGuid(), Guid.NewGuid(), 5), CancellationToken.None));
    }
}
