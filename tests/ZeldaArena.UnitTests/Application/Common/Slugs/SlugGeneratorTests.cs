using ZeldaArena.Application.Common.Slugs;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.UnitTests.Application.Common.Slugs;

public class SlugGeneratorTests
{
    private static readonly Func<Slug, CancellationToken, Task<bool>> NothingTaken = (_, _) => Task.FromResult(false);

    [Theory]
    [InlineData("Стражи Хайрула", "strazhi-khayrula")]
    [InlineData("Щит и Ёж", "shchit-i-ezh")]
    [InlineData("Hyrule Knights", "hyrule-knights")]
    [InlineData("Зора 2026", "zora-2026")]
    public async Task Cyrillic_is_transliterated_into_a_readable_slug(string name, string expected) =>
        (await SlugGenerator.UniqueAsync(name, "TAG", NothingTaken, CancellationToken.None)).Value.ShouldBe(expected);

    /// <summary>Из одних эмодзи слаг не складывается — берётся запасной текст (тег).</summary>
    [Fact]
    public async Task Fallback_is_used_when_nothing_is_left_of_the_name() =>
        (await SlugGenerator.UniqueAsync("🔥🔥🔥", "FIRE", NothingTaken, CancellationToken.None)).Value.ShouldBe("fire");

    [Fact]
    public async Task Taken_slug_gets_the_next_free_number()
    {
        var taken = new HashSet<string> { "hyrule-knights", "hyrule-knights-2" };

        var slug = await SlugGenerator.UniqueAsync(
            "Hyrule Knights",
            "HYR",
            (candidate, _) => Task.FromResult(taken.Contains(candidate.Value)),
            CancellationToken.None);

        slug.Value.ShouldBe("hyrule-knights-3");
    }

    /// <summary>«/teams/create» — страница создания; команда с таким слагом была бы недостижима.</summary>
    [Fact]
    public async Task Route_words_are_never_issued() =>
        (await SlugGenerator.UniqueAsync("Create", "CRT", NothingTaken, CancellationToken.None)).Value.ShouldBe("create-2");

    [Fact]
    public async Task Very_long_name_leaves_room_for_a_suffix()
    {
        var slug = await SlugGenerator.UniqueAsync(new string('a', 300), "TAG", NothingTaken, CancellationToken.None);

        slug.Value.Length.ShouldBeLessThanOrEqualTo(Slug.MaxLength);
    }

    [Fact]
    public async Task Neither_name_nor_fallback_usable_is_a_programming_error() =>
        await Should.ThrowAsync<ArgumentException>(() =>
            SlugGenerator.UniqueAsync("🔥", "🔥", NothingTaken, CancellationToken.None));
}