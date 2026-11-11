using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Features.Auth.LogoutAllDvices;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class LogoutAllDevicesHandlerTests
{
    private readonly Mock<IUserRefreshTokenReposiory> _refreshTokenRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly LogoutAllDevicesHandler _handler;

    public LogoutAllDevicesHandlerTests()
    {
        _refreshTokenRepositoryMock = new Mock<IUserRefreshTokenReposiory>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _unitOfWorkMock
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> operation, CancellationToken ct) => operation(ct));

        _handler = new LogoutAllDevicesHandler(
            _refreshTokenRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldIncrementTokenVersionAndRevokeAllTokens()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, TokenVersion = 3 };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _refreshTokenRepositoryMock
            .Setup(r => r.RevokeAllByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        await _handler.Handle(new LogoutAllDevicesCommand(userId), CancellationToken.None);

        Assert.Equal(4, user.TokenVersion);
        _refreshTokenRepositoryMock.Verify(
            r => r.RevokeAllByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserDoesNotExist()
    {
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<Exception>(
            () => _handler.Handle(new LogoutAllDevicesCommand(Guid.NewGuid()), CancellationToken.None));

        _refreshTokenRepositoryMock.Verify(
            r => r.RevokeAllByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
