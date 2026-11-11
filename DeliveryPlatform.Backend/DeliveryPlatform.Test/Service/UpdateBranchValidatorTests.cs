using DeliveryPlatform.Application.src.Features.Branch.Update;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class UpdateBranchValidatorTests
{
    private readonly UpdateBranchValidator _validator = new();

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
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateBranchCommand.Name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenAddressIsEmpty(string address)
    {
        var command = CreateCommand() with { Address = address };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateBranchCommand.Address));
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void Validate_ShouldFail_WhenLatitudeIsOutOfRange(double latitude)
    {
        var command = CreateCommand() with { Latitude = latitude };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateBranchCommand.Latitude));
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public void Validate_ShouldFail_WhenLongitudeIsOutOfRange(double longitude)
    {
        var command = CreateCommand() with { Longitude = longitude };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateBranchCommand.Longitude));
    }

    private static UpdateBranchCommand CreateCommand() => new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        "Main Branch",
        "123 Main St",
        31.9539,
        35.9106,
        true,
        Guid.NewGuid());
}
