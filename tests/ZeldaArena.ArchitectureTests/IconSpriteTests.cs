using System.Text.RegularExpressions;

namespace ZeldaArena.ArchitectureTests;

/// <summary>
/// Иконки — SVG-спрайт (docs/design/design-system.md §12), и опечатка в имени символа
/// ничего не ломает: <c>&lt;use href="#serach"&gt;</c> собирается, не бросает исключение
/// и просто не рисует ничего. Заметить это можно было бы только глазами и только
/// на той странице, где иконка встречается, — поэтому проверка автоматическая.
///
/// Правило двустороннее. Константа без символа — невидимая иконка на странице.
/// Символ без константы — мёртвый вес в файле, который раздаётся каждому посетителю
/// и противоречит §16: переносим только то, что реально используется.
/// </summary>
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

    /// <summary>
    /// Обратная сторона: проверка должна что-то находить. Пустой спрайт или пустой
    /// список констант сделал бы оба теста вечнозелёными — ровно та вакуумность,
    /// от которой страхует sentinel-тест из §5.2.
    /// </summary>
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

    /// <summary>
    /// Константы читаются из исходника, а не через рефлексию: <c>ZeldaArena.Web</c>
    /// в архитектурных тестах уже загружается, но <c>SpritePath</c> — тоже строковая
    /// константа этого класса, и её выборка вместе с именами иконок дала бы ложное
    /// расхождение. Разбор исходника позволяет взять ровно объявления иконок.
    /// </summary>
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

    /// <summary>
    /// Только объявления вида <c>public const string Search = "search";</c>.
    /// <c>SpritePath</c> под шаблон не подходит: его значение начинается со слэша.
    /// </summary>
    [GeneratedRegex(
        """public\s+const\s+string\s+\w+\s*=\s*"([a-z][a-z0-9-]*)"\s*;""",
        RegexOptions.None,
        matchTimeoutMilliseconds: 2000)]
    private static partial Regex IconConstant();
}