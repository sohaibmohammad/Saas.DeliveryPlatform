using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Features.Business.Update;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class UpdateBusinessHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateBusinessHandler _handler;

    public UpdateBusinessHandlerTests()
    {
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateBusinessHandler(_businessRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateBusiness_WhenUserIsOwner()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var business = new Business
        {
            Id = businessId,
            UserId = userId,
            Name = "OldName",
            Description = "OldDesc",
            BusinessType = BusinessType.Grocery,
            IsActive = true
        };

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UpdateBusinessCommand(
            businessId,
            "NewName",
            "NewDesc",
            BusinessType.Restaurant,
            false,
            userId);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(businessId, result);
        Assert.Equal("NewName", business.Name);
        Assert.Equal("NewDesc", business.Description);
        Assert.Equal(BusinessType.Restaurant, business.BusinessType);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldTrimNameAndClearWhitespaceDescription()
    {
        var userId = Guid.NewGuid();
        var businessId = Guid.NewGuid();
        var business = new Business
        {
            Id = businessId,
            UserId = userId,
            Name = "OldName",
            Description = "OldDesc",
            BusinessType = BusinessType.Grocery
        };

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(userId, businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UpdateBusinessCommand(
            businessId,
            "  NewName  ",
            "   ",
            BusinessType.Restaurant,
            true,
            userId);

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal("NewName", business.Name);
        Assert.Null(business.Description);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenBusinessNotFoundOrNotOwned()
    {
        var businessId = Guid.NewGuid();

        _businessRepositoryMock
            .Setup(r => r.GetByUserIdAndBusinessIdAsync(It.IsAny<Guid>(), businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        var command = new UpdateBusinessCommand(
            businessId,
            "NewName",
            null,
            BusinessType.Restaurant,
            true,
            Guid.NewGuid());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenNameIsEmpty()
    {
        var command = new UpdateBusinessCommand(
            Guid.NewGuid(),
            " ",
            null,
            BusinessType.Restaurant,
            true,
            Guid.NewGuid());

        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
        _businessRepositoryMock.Verify(
            r => r.GetByUserIdAndBusinessIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenBusinessTypeIsInvalid()
    {
        var command = new UpdateBusinessCommand(
            Guid.NewGuid(),
            "NewName",
            null,
            (BusinessType)999,
            true,
            Guid.NewGuid());

        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
        _businessRepositoryMock.Verify(
            r => r.GetByUserIdAndBusinessIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
