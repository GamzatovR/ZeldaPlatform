using ZeldaArena.Domain.Common;

namespace ZeldaArena.Domain.Events;

/// <summary>Заказ оформлен: повод для письма-подтверждения и уведомления в колокольчик.</summary>
public sealed record OrderPlacedEvent(
    Guid OrderId,
    Guid UserId,
    string Number,
    decimal Total,
    string Currency) : DomainEvent;