using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Features.Branch.Create;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class CreateBranchHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly Mock<IBranchRepository> _branchRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateBranchHandler _handler;

    public CreateBranchHandlerTests()
    {
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _branchRepositoryMock = new Mock<IBranchRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateBranchHandler(
            _businessRepositoryMock.Object,
            _branchRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateBranch_WhenUserOwnsBusiness()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        Branch? createdBranch = null;

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId, Name = "Shop" });
        _branchRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Branch>(), It.IsAny<CancellationToken>()))
            .Callback<Branch, CancellationToken>((branch, _) => createdBranch = branch)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateBranchCommand(
            businessId,
            " Main Branch ",
            " 123 Main St ",
            31.9539,
            35.9106,
            userId);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(createdBranch);
        Assert.Equal(result, createdBranch!.Id);
        Assert.Equal(businessId, createdBranch.BusinessId);
        Assert.Equal("Main Branch", createdBranch.Name);
        Assert.Equal("123 Main St", createdBranch.Address);
        Assert.Equal(31.9539, createdBranch.Latitude);
        Assert.Equal(35.9106, createdBranch.Longitude);
        Assert.True(createdBranch.IsActive);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenBusinessIsNotOwnedByUser()
    {
        var command = new CreateBranchCommand(
            Guid.NewGuid(),
            "Main Branch",
            "123 Main St",
            31.9539,
            35.9106,
            Guid.NewGuid());

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(
                command.UserId,
                command.BusinessId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        _branchRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Branch>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
