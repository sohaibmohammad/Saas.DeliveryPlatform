using DeliveryPlatform.Application.src.Features.Category.Update;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class UpdateCategoryValidatorTests
{
    private readonly UpdateCategoryValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_ForValidCommand()
    {
        var result = _validator.Validate(
            new UpdateCategoryCommand(Guid.NewGuid(), Guid.NewGuid(), "Drinks", Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldFail_WhenBusinessIdIsEmpty()
    {
        var result = _validator.Validate(
            new UpdateCategoryCommand(Guid.Empty, Guid.NewGuid(), "Drinks", Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCategoryCommand.BusinessId));
    }

    [Fact]
    public void Validate_ShouldFail_WhenCategoryIdIsEmpty()
    {
        var result = _validator.Validate(
            new UpdateCategoryCommand(Guid.NewGuid(), Guid.Empty, "Drinks", Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCategoryCommand.CategoryId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenNameIsEmpty(string name)
    {
        var result = _validator.Validate(
            new UpdateCategoryCommand(Guid.NewGuid(), Guid.NewGuid(), name, Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCategoryCommand.Name));
    }

    [Fact]
    public void Validate_ShouldFail_WhenNameIsTooLong()
    {
        var result = _validator.Validate(
            new UpdateCategoryCommand(Guid.NewGuid(), Guid.NewGuid(), new string('a', 101), Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCategoryCommand.Name));
    }
}
