using DeliveryPlatform.Application.src.Features.Order.GetBusinessOrders;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class GetBusinessOrdersValidatorTests
{
    private readonly GetBusinessOrdersValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_ForValidQuery()
    {
        var query = new GetBusinessOrdersQuery(Guid.NewGuid(), 1, 10, null);

        var result = _validator.Validate(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldPass_WhenBranchIdIsNull()
    {
        var query = new GetBusinessOrdersQuery(Guid.NewGuid(), 1, 10, null, null);

        var result = _validator.Validate(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldFail_WhenBusinessIdIsEmpty()
    {
        var query = new GetBusinessOrdersQuery(Guid.Empty, 1, 10, null);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetBusinessOrdersQuery.BusinessId));
    }

    [Fact]
    public void Validate_ShouldFail_WhenBranchIdIsEmpty()
    {
        var query = new GetBusinessOrdersQuery(Guid.NewGuid(), 1, 10, null, Guid.Empty);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetBusinessOrdersQuery.BranchId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ShouldFail_WhenPageNumberIsNotPositive(int pageNumber)
    {
        var query = new GetBusinessOrdersQuery(Guid.NewGuid(), pageNumber, 10, null);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetBusinessOrdersQuery.PageNumber));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(GetBusinessOrdersValidator.MaxPageSize + 1)]
    public void Validate_ShouldFail_WhenPageSizeIsOutOfRange(int pageSize)
    {
        var query = new GetBusinessOrdersQuery(Guid.NewGuid(), 1, pageSize, null);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetBusinessOrdersQuery.PageSize));
    }

    [Fact]
    public void Validate_ShouldPass_WhenPageSizeEqualsMaximum()
    {
        var query = new GetBusinessOrdersQuery(Guid.NewGuid(), 1, GetBusinessOrdersValidator.MaxPageSize, null);

        var result = _validator.Validate(query);

        Assert.True(result.IsValid);
    }
}
