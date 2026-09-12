using System.Text.RegularExpressions;

namespace ZeldaArena.ArchitectureTests;

public partial class HtmlSafetyTests
{
    /// <summary>Поля, прошедшие HtmlSanitizer на входе, — единственные, кому разрешён сырой вывод.</summary>
    private static readonly string[] SanitizedFields = ["RulesHtml", "BodyHtml"];

    [Fact]
    public void Html_raw_is_used_only_for_sanitized_fields()
    {
        var offenders = Views()
            .SelectMany(view => RawCalls(view.Text).Select(argument => $"{view.Path}: Html.Raw({argument})"))
            .Where(call => !IsSanitizedField(call))
            .ToArray();

        offenders.ShouldBeEmpty(
            "Html.Raw разрешён только для RulesHtml и BodyHtml, прошедших HtmlSanitizer. "
            + $"Нарушения: {string.Join("; ", offenders)}");
    }

    [Fact]
    public void The_rule_sees_the_legitimate_calls() =>
        Views().SelectMany(view => RawCalls(view.Text)).Count().ShouldBeGreaterThanOrEqualTo(2);

    [Theory]
    [InlineData("<div>@Html.Raw(Model.Team.Description)</div>")]
    [InlineData("@Html.Raw( player.Bio )")]
    [InlineData("@Html.Raw(ViewData[\"Html\"])")]
    public void The_rule_recognises_a_real_violation(string source) =>
        RawCalls(source).ShouldAllBe(argument => !SanitizedFields.Any(field => argument.EndsWith(field, StringComparison.Ordinal)));

    [Theory]
    [InlineData("<div class=\"prose\">@Html.Raw(tournament.RulesHtml)</div>")]
    [InlineData("@Html.Raw(Model.BodyHtml)")]
    public void The_rule_leaves_sanitized_fields_alone(string source) =>
        RawCalls(source).ShouldAllBe(argument => SanitizedFields.Any(field => argument.EndsWith(field, StringComparison.Ordinal)));

    private static bool IsSanitizedField(string call) =>
        SanitizedFields.Any(field => call.EndsWith($".{field})", StringComparison.Ordinal));

    private static IEnumerable<string> RawCalls(string text) =>
        RawCall().Matches(text).Select(match => match.Groups["argument"].Value.Trim());

    [GeneratedRegex(
        """Html\.Raw\s*\((?<argument>[^)]*)\)""",
        RegexOptions.None,
        matchTimeoutMilliseconds: 2000)]
    private static partial Regex RawCall();

    private static IEnumerable<(string Path, string Text)> Views()
    {
        var root = ArchitectureFixture.SolutionRoot();

        return Directory
            .EnumerateFiles(Path.Combine(root, "src"), "*.cshtml", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
                StringComparison.Ordinal))
            .Select(path => (Path: Path.GetRelativePath(root, path), Text: File.ReadAllText(path)));
    }
}