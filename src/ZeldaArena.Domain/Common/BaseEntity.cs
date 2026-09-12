namespace ZeldaArena.Domain.Common;

/// <summary>Базовый тип сущности.</summary>
public abstract class BaseEntity : IHasDomainEvents
{
    private readonly List<DomainEvent> _domainEvents = [];

    /// <summary>Ключ версии 7.</summary>
    public Guid Id { get; protected set; } = Guid.CreateVersion7();

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void Raise(DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }
}