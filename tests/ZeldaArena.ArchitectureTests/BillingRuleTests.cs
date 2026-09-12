using System.Text.RegularExpressions;

using ZeldaArena.Domain.Constants;

namespace ZeldaArena.ArchitectureTests;

public partial class BillingRuleTests
{
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
            + "и [RequireFeature]; роль Premium — бейдж для отображения. "
            + $"Нарушения: {string.Join(", ", offenders)}");
    }

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

    private static string WithoutComments(string text) =>
        Comment().Replace(text, string.Empty);

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
        var root = ArchitectureFixture.SolutionRoot();

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

}