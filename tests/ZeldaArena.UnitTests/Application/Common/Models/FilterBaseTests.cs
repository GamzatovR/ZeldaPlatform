using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.UnitTests.Application.Common.Models;

public class FilterBaseTests
{
    [Theory]
    [InlineData(12, 12)]
    [InlineData(24, 24)]
    [InlineData(48, 48)]
    public void Allowed_page_size_passes_through(int requested, int expected) =>
        PageSizes.Normalize(requested).ShouldBe(expected);

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(25)]
    [InlineData(1000)]
    public void Page_size_outside_whitelist_falls_back_to_default(int requested) =>
        PageSizes.Normalize(requested).ShouldBe(PageSizes.Default);

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Page_below_one_is_clamped(int requested)
    {
        var filter = new TestFilter { Page = requested };

        filter.NormalizedPage.ShouldBe(1);
    }

    [Fact]
    public void Defaults_are_first_page_of_default_size()
    {
        var filter = new TestFilter();

        filter.NormalizedPage.ShouldBe(1);
        filter.NormalizedPageSize.ShouldBe(PageSizes.Default);
        filter.Sort.ShouldBeNull();
    }

    private sealed record TestFilter : FilterBase;
}