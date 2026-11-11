using DeliveryPlatform.Application.src.Features.Category.Create;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class CreateCategoryValidatorTests
{
    private readonly CreateCategoryValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_ForValidCommand()
    {
        var result = _validator.Validate(new CreateCategoryCommand(Guid.NewGuid(), "Drinks", Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldFail_WhenBusinessIdIsEmpty()
    {
        var result = _validator.Validate(new CreateCategoryCommand(Guid.Empty, "Drinks", Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCategoryCommand.BusinessId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenNameIsEmpty(string name)
    {
        var result = _validator.Validate(new CreateCategoryCommand(Guid.NewGuid(), name, Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCategoryCommand.Name));
    }

    [Fact]
    public void Validate_ShouldFail_WhenNameIsTooLong()
    {
        var result = _validator.Validate(
            new CreateCategoryCommand(Guid.NewGuid(), new string('a', 101), Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCategoryCommand.Name));
    }
}
