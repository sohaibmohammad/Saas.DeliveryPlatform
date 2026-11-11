using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Features.Branch.GetByBusiness;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class GetBranchesHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly Mock<IBranchRepository> _branchRepositoryMock;
    private readonly GetBranchesHandler _handler;

    public GetBranchesHandlerTests()
    {
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _branchRepositoryMock = new Mock<IBranchRepository>();
        _handler = new GetBranchesHandler(
            _businessRepositoryMock.Object,
            _branchRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedBranches_WhenUserOwnsBusiness()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _branchRepositoryMock
            .Setup(r => r.GetByBusinessIdAsync(businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Branch>
            {
                CreateBranch(businessId, "A"),
                CreateBranch(businessId, "B"),
                CreateBranch(businessId, "C")
            });

        var result = await _handler.Handle(
            new GetBranchesQuery(businessId, userId, false, 1, 2),
            CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(2, result.PageSize);
        Assert.Equal("A", result.Items[0].Name);
    }

    [Fact]
    public async Task Handle_ShouldUseAdminLookup_WhenIsAdmin()
    {
        var businessId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByIdAsync(businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId });
        _branchRepositoryMock
            .Setup(r => r.GetByBusinessIdAsync(businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Branch>());

        await _handler.Handle(
            new GetBranchesQuery(businessId, Guid.NewGuid(), true),
            CancellationToken.None);

        _businessRepositoryMock.Verify(
            r => r.GetByUserIdAndBusinessIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenBusinessNotFound()
    {
        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(new GetBranchesQuery(Guid.NewGuid(), Guid.NewGuid(), false), CancellationToken.None));

        _branchRepositoryMock.Verify(
            r => r.GetByBusinessIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUseDefaultPaging_WhenPagingIsInvalid()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Business { Id = businessId, UserId = userId });
        _branchRepositoryMock
            .Setup(r => r.GetByBusinessIdAsync(businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Branch> { CreateBranch(businessId, "A") });

        var result = await _handler.Handle(
            new GetBranchesQuery(businessId, userId, false, 0, 0),
            CancellationToken.None);

        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
    }

    private static Branch CreateBranch(Guid businessId, string name) => new()
    {
        Id = Guid.NewGuid(),
        BusinessId = businessId,
        Name = name,
        Address = "Address",
        Latitude = 31.0,
        Longitude = 35.0,
        IsActive = true
    };
}
