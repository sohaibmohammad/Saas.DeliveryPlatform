using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Application.src.Features.Auth.Login;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class LoginUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHashService> _passwordHashServiceMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IUserRefreshTokenReposiory> _refreshTokenRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly LoginUserHandler _handler;

    public LoginUserHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHashServiceMock = new Mock<IPasswordHashService>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _refreshTokenRepositoryMock = new Mock<IUserRefreshTokenReposiory>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new LoginUserHandler(
            _unitOfWorkMock.Object,
            _userRepositoryMock.Object,
            _passwordHashServiceMock.Object,
            _jwtTokenServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _refreshTokenRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "John",
            Email = "john@example.com",
            PasswordHash = "stored-hash"
        };
        UserRefreshToken? storedToken = null;

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHashServiceMock
            .Setup(s => s.VerifyPassword(user, "stored-hash", "Password123!"))
            .Returns(true);
        _jwtTokenServiceMock.Setup(s => s.GenerateToken(user)).Returns("access-token");
        _refreshTokenServiceMock.Setup(s => s.GenerateRefreshToken()).Returns("refresh-token");
        _refreshTokenServiceMock.Setup(s => s.HashRefreshToken("refresh-token")).Returns("refresh-hash");
        _refreshTokenRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<UserRefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<UserRefreshToken, CancellationToken>((token, _) => storedToken = token)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(
            new LoginUserCommand("  John@Example.COM ", "Password123!"),
            CancellationToken.None);

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.NotNull(storedToken);
        Assert.Equal(user.Id, storedToken!.UserId);
        Assert.Equal("refresh-hash", storedToken.TokenHash);
        Assert.Null(storedToken.RevokedAt);
        Assert.Null(storedToken.ReplacedByTokenId);
        Assert.True(storedToken.ExpiryDate > DateTime.UtcNow);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserDoesNotExist()
    {
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(new LoginUserCommand("john@example.com", "Password123!"), CancellationToken.None));

        _refreshTokenRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<UserRefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserHasNoPasswordHash()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "john@example.com",
            PasswordHash = null
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(new LoginUserCommand("john@example.com", "Password123!"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenPasswordIsInvalid()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "john@example.com",
            PasswordHash = "stored-hash"
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHashServiceMock
            .Setup(s => s.VerifyPassword(user, "stored-hash", "WrongPassword"))
            .Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(new LoginUserCommand("john@example.com", "WrongPassword"), CancellationToken.None));

        _refreshTokenRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<UserRefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
