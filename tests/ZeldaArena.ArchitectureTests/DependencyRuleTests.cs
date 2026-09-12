using System.Reflection;

using NetArchTest.Rules;

namespace ZeldaArena.ArchitectureTests;

public class DependencyRuleTests
{
    [Fact]
    public void Domain_should_not_depend_on_other_layers()
    {
        var result = Types.InAssembly(ArchitectureFixture.Domain)
            .ShouldNot()
            .HaveDependencyOnAny(
                ArchitectureFixture.ApplicationNamespace,
                ArchitectureFixture.InfrastructureNamespace,
                ArchitectureFixture.WebNamespace,
                ArchitectureFixture.EntityFrameworkNamespace,
                ArchitectureFixture.AspNetCoreNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Domain обязан оставаться на голом BCL. Нарушители: {FailingTypes(result)}");
    }

    [Fact]
    public void Domain_assembly_should_not_reference_forbidden_packages()
    {
        AssertNoReferences(
            ArchitectureFixture.Domain,
            ArchitectureFixture.ApplicationNamespace,
            ArchitectureFixture.InfrastructureNamespace,
            ArchitectureFixture.WebNamespace,
            ArchitectureFixture.EntityFrameworkNamespace,
            ArchitectureFixture.AspNetCoreNamespace);
    }

    [Fact]
    public void Application_should_not_depend_on_infrastructure_web_or_frameworks()
    {
        var result = Types.InAssembly(ArchitectureFixture.Application)
            .ShouldNot()
            .HaveDependencyOnAny(
                ArchitectureFixture.InfrastructureNamespace,
                ArchitectureFixture.WebNamespace,
                ArchitectureFixture.EntityFrameworkNamespace,
                ArchitectureFixture.AspNetCoreNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Application знает только Domain и свои порты. Нарушители: {FailingTypes(result)}");
    }

    [Fact]
    public void Application_assembly_should_not_reference_forbidden_packages()
    {
        AssertNoReferences(
            ArchitectureFixture.Application,
            ArchitectureFixture.InfrastructureNamespace,
            ArchitectureFixture.WebNamespace,
            ArchitectureFixture.EntityFrameworkNamespace,
            ArchitectureFixture.AspNetCoreNamespace);
    }

    [Fact]
    public void Infrastructure_should_not_depend_on_web()
    {
        var result = Types.InAssembly(ArchitectureFixture.Infrastructure)
            .ShouldNot()
            .HaveDependencyOn(ArchitectureFixture.WebNamespace)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Infrastructure не знает о слое представления. Нарушители: {FailingTypes(result)}");
    }

    [Fact]
    public void Web_should_not_use_infrastructure_types_outside_composition_root()
    {
        // Единственное разрешённое место — Program.cs.
        var offenders = Types.InAssembly(ArchitectureFixture.Web)
            .ShouldNot()
            .HaveDependencyOn(ArchitectureFixture.InfrastructureNamespace)
            .GetResult()
            .FailingTypes?
            .Where(type => ArchitectureFixture.RootTypeName(type) != nameof(Program))
            .Select(type => type.FullName ?? type.Name)
            .ToArray() ?? [];

        offenders.ShouldBeEmpty(
            "Типы Infrastructure допустимы только в Program.cs, всё остальное общается "
            + $"через порты Application. Нарушители: {string.Join(", ", offenders)}");
    }

    private static void AssertNoReferences(Assembly assembly, params string[] forbiddenPrefixes)
    {
        var offenders = ArchitectureFixture.ReferencedAssemblyNames(assembly)
            .Where(name => forbiddenPrefixes.Any(prefix =>
                name.StartsWith(prefix, StringComparison.Ordinal)))
            .ToArray();

        offenders.ShouldBeEmpty(
            $"Сборка {assembly.GetName().Name} не должна ссылаться на "
            + $"{string.Join(", ", forbiddenPrefixes)}. Найдено: {string.Join(", ", offenders)}");
    }

    private static string FailingTypes(TestResult result) =>
        result.FailingTypeNames is null ? "нет" : string.Join(", ", result.FailingTypeNames);
}