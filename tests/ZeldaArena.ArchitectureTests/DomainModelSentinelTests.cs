using ZeldaArena.Domain.Common;

namespace ZeldaArena.ArchitectureTests;

public class DomainModelSentinelTests
{
    /// <summary>21 сущность из.</summary>
    private const int ExpectedEntityCount = 20;

    private const int ExpectedDomainEventCount = 5;

    [Fact]
    public void Domain_assembly_should_contain_entities()
    {
        var entities = DomainTypes()
            .Where(type => typeof(BaseEntity).IsAssignableFrom(type))
            .ToArray();

        entities.Length.ShouldBeGreaterThanOrEqualTo(
            ExpectedEntityCount,
            $"Модель домена не набрана: наследников BaseEntity найдено {entities.Length}.");
    }

    [Theory]
    [InlineData("ZeldaArena.Domain.Esports")]
    [InlineData("ZeldaArena.Domain.Shop")]
    [InlineData("ZeldaArena.Domain.Billing")]
    [InlineData("ZeldaArena.Domain.Common.Entities")]
    public void Every_bounded_area_should_have_entities(string areaNamespace)
    {
        var entities = DomainTypes()
            .Where(type => typeof(BaseEntity).IsAssignableFrom(type))
            .Where(type => string.Equals(type.Namespace, areaNamespace, StringComparison.Ordinal))
            .ToArray();

        entities.ShouldNotBeEmpty($"В пространстве имён {areaNamespace} нет ни одной сущности.");
    }

    [Fact]
    public void Domain_assembly_should_contain_domain_events()
    {
        var events = DomainTypes()
            .Where(type => typeof(DomainEvent).IsAssignableFrom(type))
            .ToArray();

        events.Length.ShouldBeGreaterThanOrEqualTo(
            ExpectedDomainEventCount,
            $"Доменных событий найдено {events.Length}, ожидалось не меньше {ExpectedDomainEventCount}.");
    }

    [Fact]
    public void Domain_assembly_should_contain_value_objects()
    {
        DomainTypes()
            .Where(type => typeof(ValueObject).IsAssignableFrom(type))
            .ShouldNotBeEmpty("В домене нет ни одного объекта-значения.");
    }

    private static IEnumerable<Type> DomainTypes() =>
        ArchitectureFixture.Domain.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .Where(type => !ArchitectureFixture.IsCompilerGenerated(type));
}