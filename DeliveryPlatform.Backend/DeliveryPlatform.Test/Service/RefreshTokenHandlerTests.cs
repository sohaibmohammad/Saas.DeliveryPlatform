using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Application.src.Features.Auth.Refresh;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class RefreshTokenHandlerTests
{
    private readonly Mock<IUserRefreshTokenReposiory> _refreshTokenRepositoryMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RefreshTokenHandler _handler;

    public RefreshTokenHandlerTests()
    {
        _refreshTokenRepositoryMock = new Mock<IUserRefreshTokenReposiory>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new RefreshTokenHandler(
            _refreshTokenRepositoryMock.Object,
            _refreshTokenServiceMock.Object,
            _jwtTokenServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRotateToken_WhenRefreshTokenIsValid()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "john@example.com" };
        var oldToken = new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "old-hash",
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            User = user
        };
        UserRefreshToken? newToken = null;

        _refreshTokenServiceMock.Setup(s => s.HashRefreshToken("old-refresh")).Returns("old-hash");
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenHashAsync("old-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldToken);
        _jwtTokenServiceMock.Setup(s => s.GenerateToken(user)).Returns("new-access");
        _refreshTokenServiceMock.Setup(s => s.GenerateRefreshToken()).Returns("new-refresh");
        _refreshTokenServiceMock.Setup(s => s.HashRefreshToken("new-refresh")).Returns("new-hash");
        _refreshTokenRepositoryMock
            .Setup(r => r.TryRevokeAsync(oldToken.Id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _refreshTokenRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<UserRefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<UserRefreshToken, CancellationToken>((token, _) => newToken = token)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(new RefreshTokenCommand("old-refresh"), CancellationToken.None);

        Assert.Equal("new-access", result.AccessToken);
        Assert.Equal("new-refresh", result.RefreshToken);
        Assert.NotNull(newToken);
        Assert.Equal("new-hash", newToken!.TokenHash);
        Assert.Equal(user.Id, newToken.UserId);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenRefreshTokenDoesNotExist()
    {
        _refreshTokenServiceMock.Setup(s => s.HashRefreshToken(It.IsAny<string>())).Returns("hash");
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenHashAsync("hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserRefreshToken?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(new RefreshTokenCommand("token"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenRefreshTokenIsRevoked()
    {
        var token = new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = "hash",
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            RevokedAt = DateTime.UtcNow,
            User = new User { Id = Guid.NewGuid() }
        };

        _refreshTokenServiceMock.Setup(s => s.HashRefreshToken(It.IsAny<string>())).Returns("hash");
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenHashAsync("hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(new RefreshTokenCommand("token"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenRefreshTokenIsExpired()
    {
        var token = new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = "hash",
            ExpiryDate = DateTime.UtcNow.AddDays(-1),
            User = new User { Id = Guid.NewGuid() }
        };

        _refreshTokenServiceMock.Setup(s => s.HashRefreshToken(It.IsAny<string>())).Returns("hash");
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenHashAsync("hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(new RefreshTokenCommand("token"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenRefreshTokenUserIsMissing()
    {
        var token = new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = "hash",
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            User = null!
        };

        _refreshTokenServiceMock.Setup(s => s.HashRefreshToken(It.IsAny<string>())).Returns("hash");
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenHashAsync("hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(new RefreshTokenCommand("token"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenTokenWasAlreadyUsed()
    {
        var user = new User { Id = Guid.NewGuid() };
        var token = new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = "hash",
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            User = user
        };

        _refreshTokenServiceMock.Setup(s => s.HashRefreshToken(It.IsAny<string>())).Returns("hash");
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenHashAsync("hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        _jwtTokenServiceMock.Setup(s => s.GenerateToken(user)).Returns("access");
        _refreshTokenServiceMock.Setup(s => s.GenerateRefreshToken()).Returns("new-refresh");
        _refreshTokenServiceMock.Setup(s => s.HashRefreshToken("new-refresh")).Returns("new-hash");
        _refreshTokenRepositoryMock
            .Setup(r => r.TryRevokeAsync(token.Id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(new RefreshTokenCommand("token"), CancellationToken.None));

        _refreshTokenRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<UserRefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
