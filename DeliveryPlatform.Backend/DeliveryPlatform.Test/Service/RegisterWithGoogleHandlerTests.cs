using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Application.src.Features.Auth.GoogleRegister;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class RegisterWithGoogleHandlerTests
{
    private readonly Mock<IGoogleAuthService> _googleAuthServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IExternalLoginRepository> _externalLoginRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly RegisterWithGoogleHandler _handler;

    public RegisterWithGoogleHandlerTests()
    {
        _googleAuthServiceMock = new Mock<IGoogleAuthService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _externalLoginRepositoryMock = new Mock<IExternalLoginRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();

        _handler = new RegisterWithGoogleHandler(
            _googleAuthServiceMock.Object,
            _userRepositoryMock.Object,
            _externalLoginRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _jwtTokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRegisterUserAndReturnToken()
    {
        var googleUser = new GoogleUserInfo("google-subject", "john@example.com", true);
        User? createdUser = null;
        ExternalLogins? createdLogin = null;

        _googleAuthServiceMock
            .Setup(s => s.ValidateIdTokenAsync("id-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(googleUser);
        _externalLoginRepositoryMock
            .Setup(r => r.GetUserByProviderAsync("Google", "google-subject", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => createdUser = user)
            .Returns(Task.CompletedTask);
        _externalLoginRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ExternalLogins>(), It.IsAny<CancellationToken>()))
            .Callback<ExternalLogins, CancellationToken>((login, _) => createdLogin = login)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _jwtTokenServiceMock
            .Setup(s => s.GenerateToken(It.IsAny<User>()))
            .Returns("jwt-token");

        var result = await _handler.Handle(
            new RegisterWithGoogleCommand("id-token"),
            CancellationToken.None);

        Assert.Equal("jwt-token", result);
        Assert.NotNull(createdUser);
        Assert.Equal("john@example.com", createdUser!.Email);
        Assert.Equal(UserRole.Customer, createdUser.Role);
        Assert.Null(createdUser.PasswordHash);
        Assert.NotNull(createdLogin);
        Assert.Equal(createdUser.Id, createdLogin!.UserId);
        Assert.Equal("Google", createdLogin.Provider);
        Assert.Equal("google-subject", createdLogin.ProviderUserId);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenGoogleTokenIsInvalid()
    {
        _googleAuthServiceMock
            .Setup(s => s.ValidateIdTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GoogleUserInfo?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(new RegisterWithGoogleCommand("bad-token"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenEmailIsNotVerified()
    {
        _googleAuthServiceMock
            .Setup(s => s.ValidateIdTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleUserInfo("subject", "john@example.com", false));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(new RegisterWithGoogleCommand("id-token"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenGoogleAccountIsAlreadyRegistered()
    {
        _googleAuthServiceMock
            .Setup(s => s.ValidateIdTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleUserInfo("subject", "john@example.com", true));
        _externalLoginRepositoryMock
            .Setup(r => r.GetUserByProviderAsync("Google", "subject", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = "john@example.com" });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new RegisterWithGoogleCommand("id-token"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenEmailAlreadyExists()
    {
        _googleAuthServiceMock
            .Setup(s => s.ValidateIdTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleUserInfo("subject", "john@example.com", true));
        _externalLoginRepositoryMock
            .Setup(r => r.GetUserByProviderAsync("Google", "subject", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = "john@example.com" });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new RegisterWithGoogleCommand("id-token"), CancellationToken.None));

        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
