using DeliveryPlatform.Application.src.Features.Branch.Create;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class CreateBranchValidatorTests
{
    private readonly CreateBranchValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_ForValidCommand()
    {
        var command = CreateCommand();

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenNameIsEmpty(string name)
    {
        var command = CreateCommand() with { Name = name };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBranchCommand.Name));
    }

    [Fact]
    public void Validate_ShouldFail_WhenNameIsTooLong()
    {
        var command = CreateCommand() with { Name = new string('a', 151) };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBranchCommand.Name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenAddressIsEmpty(string address)
    {
        var command = CreateCommand() with { Address = address };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBranchCommand.Address));
    }

    [Fact]
    public void Validate_ShouldFail_WhenAddressIsTooLong()
    {
        var command = CreateCommand() with { Address = new string('a', 501) };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBranchCommand.Address));
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void Validate_ShouldFail_WhenLatitudeIsOutOfRange(double latitude)
    {
        var command = CreateCommand() with { Latitude = latitude };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBranchCommand.Latitude));
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public void Validate_ShouldFail_WhenLongitudeIsOutOfRange(double longitude)
    {
        var command = CreateCommand() with { Longitude = longitude };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBranchCommand.Longitude));
    }

    [Fact]
    public void Validate_ShouldPass_ForBoundaryCoordinates()
    {
        var command = CreateCommand() with { Latitude = 90, Longitude = 180 };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    private static CreateBranchCommand CreateCommand() => new(
        Guid.NewGuid(),
        "Main Branch",
        "123 Main St",
        31.9539,
        35.9106,
        Guid.NewGuid());
}
