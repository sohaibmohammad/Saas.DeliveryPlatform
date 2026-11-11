using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Exceptions;
using DeliveryPlatform.Application.src.Features.Inventory.GetByBranch;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class GetBranchInventoryHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly Mock<IBranchRepository> _branchRepositoryMock;
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly GetBranchInventoryHandler _handler;

    public GetBranchInventoryHandlerTests()
    {
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _branchRepositoryMock = new Mock<IBranchRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();

        _handler = new GetBranchInventoryHandler(
            _businessRepositoryMock.Object,
            _branchRepositoryMock.Object,
            _inventoryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedInventory_WhenValid()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var items = new List<BranchInventoryResponse>
        {
            new(Guid.NewGuid(), "Product", 10, 2, 8)
        };

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _branchRepositoryMock
            .Setup(r => r.GetByBusinessIdAndBranchIdAsync(businessId, branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Branch { Id = branchId, BusinessId = businessId });
        _inventoryRepositoryMock
            .Setup(r => r.GetByBranchIdAsync(branchId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((items, 1));

        var result = await _handler.Handle(
            new GetBranchInventoryQuery(businessId, branchId, userId, 1, 10),
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task Handle_ShouldClampPaging_WhenOutOfRange()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var branchId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _branchRepositoryMock
            .Setup(r => r.GetByBusinessIdAndBranchIdAsync(businessId, branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Branch { Id = branchId, BusinessId = businessId });
        _inventoryRepositoryMock
            .Setup(r => r.GetByBranchIdAsync(branchId, 1, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<BranchInventoryResponse>(), 0));

        var result = await _handler.Handle(
            new GetBranchInventoryQuery(businessId, branchId, userId, 0, 100),
            CancellationToken.None);

        Assert.Equal(1, result.PageNumber);
        Assert.Equal(50, result.PageSize);
        _inventoryRepositoryMock.Verify(
            r => r.GetByBranchIdAsync(branchId, 1, 50, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenBusinessNotFound()
    {
        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(
                new GetBranchInventoryQuery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenBranchNotFound()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _branchRepositoryMock
            .Setup(r => r.GetByBusinessIdAndBranchIdAsync(businessId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(
                new GetBranchInventoryQuery(businessId, Guid.NewGuid(), userId),
                CancellationToken.None));

        _inventoryRepositoryMock.Verify(
            r => r.GetByBranchIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
