using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Application.src.Features.Auth.LoginWithGoogle.cs;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class LoginWithGoogleHandlerTests
{
    private readonly Mock<IGoogleAuthService> _googleAuthServiceMock;
    private readonly Mock<IExternalLoginRepository> _externalLoginRepositoryMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IUserRefreshTokenReposiory> _refreshTokenRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly LoginWithGoogleHandler _handler;

    public LoginWithGoogleHandlerTests()
    {
        _googleAuthServiceMock = new Mock<IGoogleAuthService>();
        _externalLoginRepositoryMock = new Mock<IExternalLoginRepository>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _refreshTokenRepositoryMock = new Mock<IUserRefreshTokenReposiory>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new LoginWithGoogleHandler(
            _googleAuthServiceMock.Object,
            _externalLoginRepositoryMock.Object,
            _jwtTokenServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _refreshTokenRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTokens_WhenGoogleAccountIsLinked()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "john@example.com" };
        UserRefreshToken? storedToken = null;

        _googleAuthServiceMock
            .Setup(s => s.ValidateIdTokenAsync("id-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleUserInfo("subject", "john@example.com", true));
        _externalLoginRepositoryMock
            .Setup(r => r.GetUserByProviderAsync("Google", "subject", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
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
            new LoginWithGoogleCommand("id-token"),
            CancellationToken.None);

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.NotNull(storedToken);
        Assert.Equal(user.Id, storedToken!.UserId);
        Assert.Equal("refresh-hash", storedToken.TokenHash);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenGoogleTokenIsInvalid()
    {
        _googleAuthServiceMock
            .Setup(s => s.ValidateIdTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GoogleUserInfo?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(new LoginWithGoogleCommand("bad-token"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenGoogleAccountIsNotLinked()
    {
        _googleAuthServiceMock
            .Setup(s => s.ValidateIdTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleUserInfo("subject", "john@example.com", true));
        _externalLoginRepositoryMock
            .Setup(r => r.GetUserByProviderAsync("Google", "subject", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(new LoginWithGoogleCommand("id-token"), CancellationToken.None));

        _refreshTokenRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<UserRefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
