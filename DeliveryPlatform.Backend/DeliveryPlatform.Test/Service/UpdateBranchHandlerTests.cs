using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Features.Branch.Update;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class UpdateBranchHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly Mock<IGenericRepository<Branch>> _branchRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateBranchHandler _handler;

    public UpdateBranchHandlerTests()
    {
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _branchRepositoryMock = new Mock<IGenericRepository<Branch>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateBranchHandler(
            _businessRepositoryMock.Object,
            _branchRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateBranch_WhenUserOwnsBusiness()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var branch = new Branch
        {
            Id = branchId,
            BusinessId = businessId,
            Name = "Old Branch",
            Address = "Old Address",
            Latitude = 31.0,
            Longitude = 35.0,
            IsActive = true
        };

        _branchRepositoryMock
            .Setup(r => r.GetByIdAsync(branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId, Name = "Shop" });
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UpdateBranchCommand(
            businessId,
            branchId,
            " New Branch ",
            " New Address ",
            32.0,
            36.0,
            false,
            userId);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(branchId, result);
        Assert.Equal("New Branch", branch.Name);
        Assert.Equal("New Address", branch.Address);
        Assert.Equal(32.0, branch.Latitude);
        Assert.Equal(36.0, branch.Longitude);
        Assert.False(branch.IsActive);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenBranchNotFound()
    {
        var command = new UpdateBranchCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Main Branch",
            "123 Main St",
            31.9539,
            35.9106,
            true,
            Guid.NewGuid());

        _branchRepositoryMock
            .Setup(r => r.GetByIdAsync(command.BranchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenBranchDoesNotBelongToBusiness()
    {
        var businessId = Guid.NewGuid();
        var branch = new Branch
        {
            Id = Guid.NewGuid(),
            BusinessId = Guid.NewGuid(),
            Name = "Main Branch",
            Address = "123 Main St"
        };

        _branchRepositoryMock
            .Setup(r => r.GetByIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);

        var command = new UpdateBranchCommand(
            businessId,
            branch.Id,
            "Main Branch",
            "123 Main St",
            31.9539,
            35.9106,
            true,
            Guid.NewGuid());

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserDoesNotOwnBusiness()
    {
        var businessId = Guid.NewGuid();
        var branch = new Branch
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Name = "Main Branch",
            Address = "123 Main St"
        };

        _branchRepositoryMock
            .Setup(r => r.GetByIdAsync(branch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(
                It.IsAny<Guid>(),
                businessId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        var command = new UpdateBranchCommand(
            businessId,
            branch.Id,
            "Main Branch",
            "123 Main St",
            31.9539,
            35.9106,
            true,
            Guid.NewGuid());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
