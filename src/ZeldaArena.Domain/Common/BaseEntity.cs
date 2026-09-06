namespace ZeldaArena.Domain.Common;

/// <summary>
/// Базовый тип сущности: идентичность и накопитель доменных событий.
/// Публичных сеттеров нет — состояние меняют только методы наследников,
/// защищающие инварианты (docs/SPEC.md §5.3).
/// </summary>
public abstract class BaseEntity : IHasDomainEvents
{
    private readonly List<DomainEvent> _domainEvents = [];

    /// <summary>
    /// Ключ версии 7: сортируется по времени создания, поэтому вставка не бьёт
    /// по локальности B-tree индекса (docs/adr/ADR-0003).
    /// </summary>
    public Guid Id { get; protected set; } = Guid.CreateVersion7();

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void Raise(DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }
}
