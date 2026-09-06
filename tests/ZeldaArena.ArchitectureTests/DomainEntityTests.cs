using System.Reflection;
using System.Runtime.CompilerServices;

namespace ZeldaArena.ArchitectureTests;

/// <summary>
/// Правило 5 из docs/SPEC.md §5.2: у доменных сущностей нет публичных сеттеров —
/// состояние меняют только методы, защищающие инварианты (Match.UpdateScore и прочие).
/// </summary>
public class DomainEntityTests
{
    [Fact]
    public void Domain_types_should_not_expose_public_setters()
    {
        var offenders = ArchitectureFixture.Domain.GetTypes()
            .Where(IsDomainType)
            .SelectMany(type => type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(HasPublicSetter)
            .Select(property => $"{property.DeclaringType!.Name}.{property.Name}")
            .ToArray();

        offenders.ShouldBeEmpty(
            "Состояние доменной сущности меняется только её методами. "
            + $"Свойства с публичным сеттером: {string.Join(", ", offenders)}");
    }

    private static bool IsDomainType(Type type) =>
        type is { IsClass: true, IsPublic: true, IsAbstract: false }
        && !ArchitectureFixture.IsCompilerGenerated(type);

    /// <summary>
    /// init-сеттер разрешён: он работает только при конструировании и инвариант,
    /// проверенный фабричным методом, сломать не может. Обычный set — запрещён.
    /// </summary>
    private static bool HasPublicSetter(PropertyInfo property)
    {
        if (property.SetMethod is not { IsPublic: true } setter)
        {
            return false;
        }

        var isInitOnly = setter.ReturnParameter
            .GetRequiredCustomModifiers()
            .Any(modifier => modifier == typeof(IsExternalInit));

        return !isInitOnly;
    }
}