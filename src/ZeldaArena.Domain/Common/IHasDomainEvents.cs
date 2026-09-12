namespace ZeldaArena.Domain.Common;

/// <summary>Источник доменных событий.</summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}