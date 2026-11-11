using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Application.src.Features.Auth.Logout;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class LogoutHandlerTests
{
    private readonly Mock<IUserRefreshTokenReposiory> _refreshTokenRepositoryMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<ITokenRevocationService> _tokenRevocationServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly LogoutHandler _handler;

    public LogoutHandlerTests()
    {
        _refreshTokenRepositoryMock = new Mock<IUserRefreshTokenReposiory>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _tokenRevocationServiceMock = new Mock<ITokenRevocationService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new LogoutHandler(
            _refreshTokenRepositoryMock.Object,
            _refreshTokenServiceMock.Object,
            _tokenRevocationServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRevokeRefreshTokenAndAccessToken()
    {
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMinutes(30);
        var token = new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = "hash"
        };

        _refreshTokenServiceMock.Setup(s => s.HashRefreshToken("refresh")).Returns("hash");
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenHashAsync("hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(
            new LogoutCommand("refresh", "jti-1", expiresAt, userId),
            CancellationToken.None);

        Assert.NotNull(token.RevokedAt);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _tokenRevocationServiceMock.Verify(s => s.RevokeAsync("jti-1", expiresAt), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotSave_WhenRefreshTokenIsNotFound()
    {
        _refreshTokenServiceMock.Setup(s => s.HashRefreshToken("refresh")).Returns("hash");
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenHashAsync("hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserRefreshToken?)null);

        await _handler.Handle(
            new LogoutCommand("refresh", "jti-1", DateTime.UtcNow, Guid.NewGuid()),
            CancellationToken.None);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _tokenRevocationServiceMock.Verify(
            s => s.RevokeAsync("jti-1", It.IsAny<DateTime>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotSave_WhenRefreshTokenBelongsToAnotherUser()
    {
        var token = new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = "hash"
        };

        _refreshTokenServiceMock.Setup(s => s.HashRefreshToken("refresh")).Returns("hash");
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenHashAsync("hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        await _handler.Handle(
            new LogoutCommand("refresh", "jti-1", DateTime.UtcNow, Guid.NewGuid()),
            CancellationToken.None);

        Assert.Null(token.RevokedAt);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSkipRefreshToken_WhenRefreshTokenIsEmpty()
    {
        await _handler.Handle(
            new LogoutCommand("   ", "jti-1", DateTime.UtcNow, Guid.NewGuid()),
            CancellationToken.None);

        _refreshTokenServiceMock.Verify(s => s.HashRefreshToken(It.IsAny<string>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(
            r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _tokenRevocationServiceMock.Verify(
            s => s.RevokeAsync("jti-1", It.IsAny<DateTime>()),
            Times.Once);
    }
}
