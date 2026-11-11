using DeliveryPlatform.Application.src.Features.Auth.Register;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class RegisterUserValidatorTests
{
    private readonly RegisterUserValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_ForValidCommand()
    {
        var command = new RegisterUserCommand("John Doe", "john@example.com", "Password123!");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_ShouldFail_WhenNameIsEmpty(string name)
    {
        var command = new RegisterUserCommand(name, "john@example.com", "Password123!");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.Name));
    }

    [Fact]
    public void Validate_ShouldFail_WhenNameIsTooLong()
    {
        var command = new RegisterUserCommand(new string('a', 101), "john@example.com", "Password123!");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.Name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_ShouldFail_WhenEmailIsInvalid(string email)
    {
        var command = new RegisterUserCommand("John", email, "Password123!");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.Email));
    }

    [Fact]
    public void Validate_ShouldFail_WhenEmailIsTooLong()
    {
        var command = new RegisterUserCommand(
            "John",
            $"{new string('a', 150)}@example.com",
            "Password123!");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    public void Validate_ShouldFail_WhenPasswordIsInvalid(string password)
    {
        var command = new RegisterUserCommand("John", "john@example.com", password);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }
}
