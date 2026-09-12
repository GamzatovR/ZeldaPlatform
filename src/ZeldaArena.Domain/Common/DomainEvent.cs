namespace ZeldaArena.Domain.Common;

/// <summary>Базовый тип доменного события.</summary>
public abstract record DomainEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();

    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}