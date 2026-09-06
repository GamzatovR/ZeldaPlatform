namespace ZeldaArena.Domain.Common;

/// <summary>
/// Базовый тип доменного события. Намеренно не наследует ничего из MediatR:
/// Domain остаётся на голом BCL (docs/SPEC.md §5.2), а обёртку в INotification
/// добавляет Application в Фазе 2.
/// </summary>
public abstract record DomainEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();

    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
