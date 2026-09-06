namespace ZeldaArena.Domain.Common;

/// <summary>
/// Источник доменных событий. Диспетчер Фазы 2 собирает события по этому интерфейсу,
/// не зная о конкретных сущностях.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
