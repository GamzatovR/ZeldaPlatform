using System.Text.RegularExpressions;

using ZeldaArena.Domain.Constants;

namespace ZeldaArena.ArchitectureTests;

/// <summary>
/// Правила разграничения доступа к платным функциям (docs/SPEC.md §7.4, §20 пункт 2,
/// docs/adr/ADR-0005).
///
/// Проверка идёт по исходникам, а не по собранным сборкам, по той же причине,
/// что и в <see cref="ProjectFileRuleTests"/>: нарушение здесь — это конкретная
/// строка кода, и поймать её надо в момент появления, а не когда она сработает
/// на защите.
///
/// Зачем это вообще. Роль <c>Premium</c> существует ради бейджа у ника, и соблазн
/// проверить доступ через неё велик: она рядом, она есть в cookie, она не требует
/// обращения к базе. Но тогда рассыпается вся расширяемость: снять фичу с тарифа
/// в админке (EP-4) станет нечем, потому что права будет давать роль, а не набор фич.
/// </summary>
public partial class BillingRuleTests
{
    /// <summary>
    /// Файлы, которым положено упоминать роль: там она объявлена, выдаётся, снимается
    /// и сеется. Проверять их запретом бессмысленно — они и есть исключение.
    ///
    /// Отдельно стоит <c>HeaderViewComponent.cs</c>: бейдж у ника — то самое
    /// единственное применение роли, ради которого она существует (docs/SPEC.md §7.4).
    /// Отличить «показать бейдж» от «скрыть платный блок» регулярным выражением нельзя,
    /// поэтому исключение сделано точечно, на один файл. Роль читается ровно один раз
    /// и уезжает во вьюху булевым флагом <c>ShowPremiumBadge</c>, поэтому сама разметка
    /// шапки о роли уже не знает и под исключение не попадает. Платные блоки прячет
    /// <c>&lt;feature-gate&gt;</c>, и правило продолжает следить за всеми остальными
    /// представлениями.
    ///
    /// До Фазы 5 исключением был <c>_LoginPartial.cshtml</c>; вместе с перевёрсткой
    /// шапки бейдж переехал в компонент, а сам partial удалён.
    /// </summary>
    private static readonly string[] AllowedToMentionPremium =
    [
        "RoleNames.cs",
        "SubscriptionActivatedEventHandler.cs",
        "SubscriptionExpiredEventHandler.cs",
        "IdentitySeeder.cs",
        "HeaderViewComponent.cs",
    ];

    [Fact]
    public void Premium_role_is_never_used_to_authorise()
    {
        var offenders = SourceFiles()
            .Where(file => !AllowedToMentionPremium.Contains(Path.GetFileName(file.Path)))
            .Where(file => AuthorisesByPremium(file.Text))
            .Select(file => file.Path)
            .ToArray();

        offenders.ShouldBeEmpty(
            "Доступ к платным функциям проверяется только через IEntitlementService "
            + "и [RequireFeature]; роль Premium — бейдж для отображения (docs/SPEC.md §7.4). "
            + $"Нарушения: {string.Join(", ", offenders)}");
    }

    /// <summary>
    /// Коды фич — константы <c>FeatureCodes</c>, а не литералы по коду (CLAUDE.md).
    /// Литерал переживает переименование кода молча и оставляет действие открытым
    /// для всех либо закрытым для всех — заметить это можно будет только вручную.
    /// </summary>
    [Fact]
    public void Feature_codes_are_never_written_as_literals()
    {
        var offenders = SourceFiles()
            .Where(file => Path.GetFileName(file.Path) != "FeatureCodes.cs")
            .Where(file => FeatureCodes.All.Any(code =>
                file.Text.Contains($"\"{code}\"", StringComparison.Ordinal)))
            .Select(file => file.Path)
            .ToArray();

        offenders.ShouldBeEmpty(
            "Код платной функции пишется константой FeatureCodes, а не строкой. "
            + $"Нарушения: {string.Join(", ", offenders)}");
    }

    /// <summary>
    /// Обратная сторона: запрет должен ловить настоящее нарушение, а не молчать всегда.
    /// Без этой проверки опечатка в регулярном выражении сделала бы тест вечнозелёным.
    /// </summary>
    [Theory]
    [InlineData("if (User.IsInRole(\"Premium\")) { return View(); }")]
    [InlineData("[Authorize(Roles = \"Premium\")]")]
    [InlineData("[Authorize(Roles = RoleNames.Premium)]")]
    [InlineData("policy.RequireRole(RoleNames.Premium);")]
    [InlineData("currentUser.IsInRole(RoleNames.Premium)")]
    public void The_rule_recognises_a_real_violation(string source) =>
        AuthorisesByPremium(source).ShouldBeTrue();

    [Theory]
    [InlineData("await userAccounts.AddToRoleAsync(userId, RoleNames.Premium, ct);")]
    [InlineData("// Роль Premium нужна только для бейджа у ника.")]
    [InlineData("public const string Premium = \"Premium\";")]
    public void The_rule_leaves_legitimate_mentions_alone(string source) =>
        AuthorisesByPremium(source).ShouldBeFalse();

    private static bool AuthorisesByPremium(string text) =>
        PremiumAuthorisation().IsMatch(WithoutComments(text));

    /// <summary>
    /// Комментарии вырезаются перед проверкой: сам запрет описан словами в
    /// <c>IEntitlementService</c> и <c>ICurrentUserService</c>, и без этого правило
    /// краснело бы на документации, объясняющей, чего делать нельзя.
    /// </summary>
    private static string WithoutComments(string text) =>
        Comment().Replace(text, string.Empty);

    /// <summary>
    /// Ловит три способа проверить доступ ролью: <c>IsInRole</c>, атрибут
    /// <c>[Authorize(Roles = …)]</c> и <c>RequireRole</c> в политике — как с константой,
    /// так и со строковым литералом.
    /// </summary>
    [GeneratedRegex(
        """(IsInRole\s*\(\s*(RoleNames\.Premium|"Premium")|Roles\s*=\s*(RoleNames\.Premium|"Premium")|RequireRole\s*\([^)]*(RoleNames\.Premium|"Premium"))""",
        RegexOptions.None,
        matchTimeoutMilliseconds: 2000)]
    private static partial Regex PremiumAuthorisation();

    /// <summary>Однострочные и блочные комментарии, включая XML-документацию.</summary>
    [GeneratedRegex(
        """//[^\r\n]*|/\*.*?\*/""",
        RegexOptions.Singleline,
        matchTimeoutMilliseconds: 2000)]
    private static partial Regex Comment();

    private static IEnumerable<(string Path, string Text)> SourceFiles()
    {
        var root = SolutionRoot();

        return Directory
            .EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(
                Path.Combine(root, "src"),
                "*.cshtml",
                SearchOption.AllDirectories))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
                StringComparison.Ordinal))
            .Select(path => (Path: Path.GetRelativePath(root, path), Text: File.ReadAllText(path)));
    }

    private static string SolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ZeldaArena.sln")))
        {
            directory = directory.Parent;
        }

        directory.ShouldNotBeNull("Не найден корень решения: ZeldaArena.sln выше каталога сборки нет.");

        return directory.FullName;
    }
}