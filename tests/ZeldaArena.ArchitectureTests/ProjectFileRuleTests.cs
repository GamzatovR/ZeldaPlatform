using System.Xml.Linq;

namespace ZeldaArena.ArchitectureTests;

/// <summary>
/// Правило 2 из docs/SPEC.md §5.2 на уровне файлов проекта.
///
/// Зачем ещё одна проверка. Существующие смотрят на собранную сборку: одна по типам,
/// другая по <c>GetReferencedAssemblies</c>. Обе молчат, если запрещённый пакет
/// подключён, но им пока никто не воспользовался, — компилятор просто не выписывает
/// ссылку на неиспользуемую сборку. Проверено пробой в Фазе 2: EF Core, добавленный
/// в ZeldaArena.Application.csproj и не использованный ни одной строкой, обе проверки
/// пропустили.
///
/// Пропущенная ссылка опасна не сама по себе, а тем, что снимает препятствие: следующий,
/// кто откроет проект, увидит EF Core в списке пакетов и решит, что так и надо.
/// Эта проверка читает сами файлы проектов, поэтому ловит ссылку в момент появления.
/// </summary>
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

    /// <summary>
    /// Domain остаётся на голом BCL: у него нет вообще никаких пакетов, а единственная
    /// допустимая ссылка на проект — отсутствует.
    /// </summary>
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

    /// <summary>
    /// Путь к файлу проекта от каталога сборки тестов: подниматься вверх до решения.
    /// Так тест не зависит ни от способа запуска, ни от рабочего каталога.
    /// </summary>
    private static string ProjectFilePath(string project)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ZeldaArena.sln")))
        {
            directory = directory.Parent;
        }

        directory.ShouldNotBeNull("Не найден корень решения: ZeldaArena.sln выше каталога сборки нет.");

        var path = Path.Combine(directory.FullName, "src", project, $"{project}.csproj");

        File.Exists(path).ShouldBeTrue($"Файл проекта не найден: {path}");

        return path;
    }
}