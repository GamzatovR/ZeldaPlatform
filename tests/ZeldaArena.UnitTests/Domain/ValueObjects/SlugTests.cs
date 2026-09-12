using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.UnitTests.Domain.ValueObjects;

public class SlugTests
{
    [Theory]
    [InlineData("Zelda Masters 2025", "zelda-masters-2025")]
    [InlineData("  Hyrule   Knights  ", "hyrule-knights")]
    [InlineData("Team_Kakariko", "team-kakariko")]
    [InlineData("already-a-slug", "already-a-slug")]
    [InlineData("--dashes--everywhere--", "dashes-everywhere")]
    public void Text_is_normalized(string input, string expected)
    {
        Slug.From(input).Value.ShouldBe(expected);
    }

    [Fact]
    public void Empty_result_is_rejected()
    {
        // Кириллица не транслитерируется: слаг обязан быть латинским.
        Should.Throw<InvariantViolationException>(() => Slug.From("Ссылка"))
            .Code.ShouldBe("slug.empty");
    }

    [Fact]
    public void Too_long_slug_is_rejected()
    {
        Should.Throw<InvariantViolationException>(() => Slug.From(new string('a', Slug.MaxLength + 1)))
            .Code.ShouldBe("slug.too_long");
    }

    [Fact]
    public void Null_or_whitespace_is_rejected()
    {
        Should.Throw<ArgumentException>(() => Slug.From("   "));
    }

    [Fact]
    public void TryFrom_reports_failure_without_throwing()
    {
        Slug.TryFrom("Ссылка", out var slug).ShouldBeFalse();
        slug.ShouldBeNull();

        Slug.TryFrom("Zelda Cup", out var valid).ShouldBeTrue();
        valid!.Value.ShouldBe("zelda-cup");
    }

    [Fact]
    public void Equality_is_structural()
    {
        Slug.From("zelda-cup").ShouldBe(Slug.From("Zelda Cup"));
    }
}