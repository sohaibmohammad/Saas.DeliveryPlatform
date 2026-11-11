using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Features.Branch.GetById;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class GetBranchByIdHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly Mock<IBranchRepository> _branchRepositoryMock;
    private readonly GetBranchByIdHandler _handler;

    public GetBranchByIdHandlerTests()
    {
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _branchRepositoryMock = new Mock<IBranchRepository>();
        _handler = new GetBranchByIdHandler(
            _businessRepositoryMock.Object,
            _branchRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnBranch_WhenFound()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var branchId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _branchRepositoryMock
            .Setup(r => r.GetByBusinessIdAndBranchIdAsync(businessId, branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Branch
            {
                Id = branchId,
                BusinessId = businessId,
                Name = "Main",
                Address = "Address",
                Latitude = 31.0,
                Longitude = 35.0,
                IsActive = true
            });

        var result = await _handler.Handle(
            new GetBranchByIdQuery(businessId, branchId, userId, false),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(branchId, result!.Id);
        Assert.Equal("Main", result.Name);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenBusinessNotFound()
    {
        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        var result = await _handler.Handle(
            new GetBranchByIdQuery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), false),
            CancellationToken.None);

        Assert.Null(result);
        _branchRepositoryMock.Verify(
            r => r.GetByBusinessIdAndBranchIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenBranchNotFound()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _branchRepositoryMock
            .Setup(r => r.GetByBusinessIdAndBranchIdAsync(businessId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Branch?)null);

        var result = await _handler.Handle(
            new GetBranchByIdQuery(businessId, Guid.NewGuid(), userId, false),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ShouldUseAdminLookup_WhenIsAdmin()
    {
        var businessId = Guid.NewGuid();
        var branchId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByIdAsync(businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId });
        _branchRepositoryMock
            .Setup(r => r.GetByBusinessIdAndBranchIdAsync(businessId, branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Branch { Id = branchId, BusinessId = businessId, Name = "Main", Address = "Address" });

        var result = await _handler.Handle(
            new GetBranchByIdQuery(businessId, branchId, Guid.NewGuid(), true),
            CancellationToken.None);

        Assert.NotNull(result);
        _businessRepositoryMock.Verify(
            r => r.GetByUserIdAndBusinessIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
