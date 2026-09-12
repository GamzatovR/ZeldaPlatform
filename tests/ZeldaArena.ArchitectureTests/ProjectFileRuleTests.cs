using System.Xml.Linq;

namespace ZeldaArena.ArchitectureTests;

public class ProjectFileRuleTests
{
    private static readonly string[] ForbiddenPackagePrefixes =
    [
        ArchitectureFixture.EntityFrameworkNamespace,
        ArchitectureFixture.AspNetCoreNamespace,
        "Npgsql",
        "MongoDB",
    ];

    [Theory]
    [InlineData("ZeldaArena.Domain")]
    [InlineData("ZeldaArena.Application")]
    public void Inner_layer_project_should_not_reference_forbidden_packages(string project)
    {
        var document = XDocument.Load(ProjectFilePath(project));

        var offenders = document.Descendants()
            .Where(element => element.Name.LocalName is "PackageReference" or "FrameworkReference")
            .Select(element => element.Attribute("Include")?.Value ?? string.Empty)
            .Where(name => Array.Exists(
                ForbiddenPackagePrefixes,
                prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        offenders.ShouldBeEmpty(
            $"{project}.csproj не должен ссылаться на инфраструктурные пакеты. "
            + $"Найдено: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void Domain_project_should_have_no_dependencies_at_all()
    {
        var document = XDocument.Load(ProjectFilePath("ZeldaArena.Domain"));

        var references = document.Descendants()
            .Where(element => element.Name.LocalName
                is "PackageReference" or "FrameworkReference" or "ProjectReference")
            .Select(element => element.Attribute("Include")?.Value ?? string.Empty)
            .ToArray();

        references.ShouldBeEmpty(
            $"Domain обязан оставаться без зависимостей. Найдено: {string.Join(", ", references)}");
    }

    private static string ProjectFilePath(string project)
    {
        var path = Path.Combine(
            ArchitectureFixture.SolutionRoot(), "src", project, $"{project}.csproj");

        File.Exists(path).ShouldBeTrue($"Файл проекта не найден: {path}");

        return path;
    }
}