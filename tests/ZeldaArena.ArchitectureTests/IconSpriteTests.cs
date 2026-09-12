using System.Text.RegularExpressions;

namespace ZeldaArena.ArchitectureTests;

public partial class IconSpriteTests
{
    [Fact]
    public void Every_icon_name_has_a_symbol_in_the_sprite()
    {
        var missing = ConstantNames().Except(SpriteSymbolIds(), StringComparer.Ordinal).ToArray();

        missing.ShouldBeEmpty(
            "В IconNames объявлены иконки, которых нет в wwwroot/img/icons.svg: "
            + $"{string.Join(", ", missing)}");
    }

    [Fact]
    public void Every_symbol_in_the_sprite_has_a_name_constant()
    {
        var orphans = SpriteSymbolIds().Except(ConstantNames(), StringComparer.Ordinal).ToArray();

        orphans.ShouldBeEmpty(
            "В спрайте лежат символы, на которые никто не ссылается через IconNames: "
            + $"{string.Join(", ", orphans)}");
    }

    [Fact]
    public void The_sprite_and_the_constants_are_not_empty()
    {
        SpriteSymbolIds().Length.ShouldBeGreaterThan(20);
        ConstantNames().Length.ShouldBeGreaterThan(20);
    }

    private static string[] SpriteSymbolIds() =>
        [.. SymbolId()
            .Matches(File.ReadAllText(SpritePath()))
            .Select(match => match.Groups[1].Value)];

    private static string[] ConstantNames() =>
        [.. IconConstant()
            .Matches(File.ReadAllText(Path.Combine(WebRoot(), "Constants", "IconNames.cs")))
            .Select(match => match.Groups[1].Value)];

    private static string SpritePath() =>
        Path.Combine(WebRoot(), "wwwroot", "img", "icons.svg");

    private static string WebRoot() =>
        Path.Combine(ArchitectureFixture.SolutionRoot(), "src", "ZeldaArena.Web");

    [GeneratedRegex(
        // Закрывающая кавычка в шаблон не входит: [^"]+ и так останавливается на ней,
        // а литерал, оканчивающийся кавычкой, потребовал бы четырёх ограничителей.
        """<symbol\s+id="([^"]+)""",
        RegexOptions.None,
        matchTimeoutMilliseconds: 2000)]
    private static partial Regex SymbolId();

    [GeneratedRegex(
        """public\s+const\s+string\s+\w+\s*=\s*"([a-z][a-z0-9-]*)"\s*;""",
        RegexOptions.None,
        matchTimeoutMilliseconds: 2000)]
    private static partial Regex IconConstant();
}