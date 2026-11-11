using DeliveryPlatform.Application.src.Common.Paging;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_ShouldBeZero_WhenPageSizeIsZero()
    {
        var result = new PagedResult<string> { PageSize = 0, TotalCount = 10 };

        Assert.Equal(0, result.TotalPages);
    }

    [Theory]
    [InlineData(10, 3, 4)]
    [InlineData(9, 3, 3)]
    [InlineData(0, 10, 0)]
    public void TotalPages_ShouldUseCeiling(int totalCount, int pageSize, int expected)
    {
        var result = new PagedResult<string> { TotalCount = totalCount, PageSize = pageSize };

        Assert.Equal(expected, result.TotalPages);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    public void HasPrevious_ShouldReflectPageNumber(int pageNumber, bool expected)
    {
        var result = new PagedResult<string> { PageNumber = pageNumber };

        Assert.Equal(expected, result.HasPrevious);
    }

    [Theory]
    [InlineData(1, 10, 30, true)]
    [InlineData(3, 10, 30, false)]
    [InlineData(4, 10, 30, false)]
    public void HasNext_ShouldReflectRemainingPages(int pageNumber, int pageSize, int totalCount, bool expected)
    {
        var result = new PagedResult<string>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        Assert.Equal(expected, result.HasNext);
    }
}
