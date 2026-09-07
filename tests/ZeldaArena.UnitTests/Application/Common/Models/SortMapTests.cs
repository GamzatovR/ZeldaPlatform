using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.UnitTests.Application.Common.Models;

public class SortMapTests
{
    private static readonly int[] Values = [3, 1, 2];

    private static SortMap<int> Map() => new SortMap<int>()
        .Add("asc", query => query.OrderBy(value => value), isDefault: true)
        .Add("desc", query => query.OrderByDescending(value => value));

    [Fact]
    public void Known_key_is_applied()
    {
        var sorted = Map().Apply(Values.AsQueryable(), "desc").ToArray();

        sorted.ShouldBe([3, 2, 1]);
    }

    [Fact]
    public void Key_is_case_insensitive()
    {
        var sorted = Map().Apply(Values.AsQueryable(), "DESC").ToArray();

        sorted.ShouldBe([3, 2, 1]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("prize_desc; drop table Tournaments")]
    [InlineData("Name")]
    public void Unknown_key_falls_back_to_default_instead_of_failing(string? key)
    {
        var sorted = Map().Apply(Values.AsQueryable(), key).ToArray();

        sorted.ShouldBe([1, 2, 3]);
    }

    [Fact]
    public void Map_without_default_cannot_sort()
    {
        var map = new SortMap<int>().Add("asc", query => query.OrderBy(value => value));

        Should.Throw<InvalidOperationException>(() => map.Apply(Values.AsQueryable(), "unknown"));
    }

    [Fact]
    public void Duplicate_key_is_a_programming_error()
    {
        var map = new SortMap<int>().Add("asc", query => query.OrderBy(value => value));

        Should.Throw<ArgumentException>(
            () => map.Add("asc", query => query.OrderByDescending(value => value)));
    }
}