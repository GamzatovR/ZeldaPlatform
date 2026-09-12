using System.Reflection;
using System.Runtime.CompilerServices;

namespace ZeldaArena.ArchitectureTests;

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