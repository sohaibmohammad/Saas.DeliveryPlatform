using DeliveryPlatform.Application.src.Features.Product.Create;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_ForValidCommand()
    {
        var result = _validator.Validate(CreateCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldFail_WhenBusinessIdIsEmpty()
    {
        var result = _validator.Validate(CreateCommand() with { BusinessId = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProductCommand.BusinessId));
    }

    [Fact]
    public void Validate_ShouldFail_WhenCategoryIdIsEmpty()
    {
        var result = _validator.Validate(CreateCommand() with { CategoryId = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProductCommand.CategoryId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenNameIsEmpty(string name)
    {
        var result = _validator.Validate(CreateCommand() with { Name = name });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProductCommand.Name));
    }

    [Fact]
    public void Validate_ShouldFail_WhenNameIsTooLong()
    {
        var result = _validator.Validate(CreateCommand() with { Name = new string('a', 201) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProductCommand.Name));
    }

    [Fact]
    public void Validate_ShouldFail_WhenDescriptionIsTooLong()
    {
        var result = _validator.Validate(CreateCommand() with { Description = new string('a', 1001) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProductCommand.Description));
    }

    [Fact]
    public void Validate_ShouldFail_WhenPriceIsNegative()
    {
        var result = _validator.Validate(CreateCommand() with { Price = -1m });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProductCommand.Price));
    }

    [Fact]
    public void Validate_ShouldPass_WhenPriceIsZero()
    {
        var result = _validator.Validate(CreateCommand() with { Price = 0m });

        Assert.True(result.IsValid);
    }

    private static CreateProductCommand CreateCommand() => new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        "Burger",
        "Tasty",
        12.5m,
        true,
        Guid.NewGuid());
}
