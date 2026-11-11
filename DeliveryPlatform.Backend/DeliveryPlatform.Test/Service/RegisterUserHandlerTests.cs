using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Application.src.Features.Auth.Register;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class RegisterUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHashService> _passwordHashServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RegisterUserHandler _handler;

    public RegisterUserHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHashServiceMock = new Mock<IPasswordHashService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new RegisterUserHandler(
            _userRepositoryMock.Object,
            _passwordHashServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRegisterUser_WhenEmailIsNotRegistered()
    {
        User? createdUser = null;
        var command = new RegisterUserCommand("John Doe", "  John@Example.COM  ", "Password123!");

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHashServiceMock
            .Setup(s => s.HashPassword(It.IsAny<User>(), command.Password))
            .Returns("hashed-password");
        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => createdUser = user)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(createdUser);
        Assert.Equal(result, createdUser!.Id);
        Assert.Equal("John Doe", createdUser.Name);
        Assert.Equal("john@example.com", createdUser.Email);
        Assert.Equal("hashed-password", createdUser.PasswordHash);
        Assert.Equal(UserRole.Customer, createdUser.Role);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenEmailAlreadyRegistered()
    {
        var command = new RegisterUserCommand("John", "john@example.com", "Password123!");

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));

        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
