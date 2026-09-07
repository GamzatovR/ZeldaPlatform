using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.UnitTests.Application.Common.Models;

public class PagedResultTests
{
    [Theory]
    [InlineData(0, 12, 1)]
    [InlineData(1, 12, 1)]
    [InlineData(12, 12, 1)]
    [InlineData(13, 12, 2)]
    [InlineData(24, 12, 2)]
    [InlineData(25, 12, 3)]
    public void Total_pages_is_calculated_from_count_and_size(
        int totalCount,
        int pageSize,
        int expectedPages)
    {
        var result = new PagedResult<int>([], 1, pageSize, totalCount);

        result.TotalPages.ShouldBe(expectedPages);
    }

    [Fact]
    public void Empty_result_is_one_empty_page_not_zero_pages()
    {
        var result = PagedResult<int>.Empty(1, 12);

        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
        result.TotalPages.ShouldBe(1);
        result.HasNext.ShouldBeFalse();
        result.HasPrevious.ShouldBeFalse();
    }

    [Fact]
    public void Middle_page_has_both_neighbours()
    {
        var result = new PagedResult<int>([1, 2], 2, 2, 6);

        result.TotalPages.ShouldBe(3);
        result.HasPrevious.ShouldBeTrue();
        result.HasNext.ShouldBeTrue();
    }

    [Fact]
    public void Last_page_has_no_next()
    {
        var result = new PagedResult<int>([1], 3, 2, 5);

        result.HasNext.ShouldBeFalse();
        result.HasPrevious.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0, 12, 0)]
    [InlineData(1, 0, 0)]
    [InlineData(1, 12, -1)]
    public void Impossible_page_is_rejected(int page, int pageSize, int totalCount)
    {
        Should.Throw<ArgumentOutOfRangeException>(
            () => new PagedResult<int>([], page, pageSize, totalCount));
    }
}