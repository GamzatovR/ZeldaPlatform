using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Domain.Events;

/// <summary>Оплата подтверждена кодом из письма.</summary>
public sealed record PaymentConfirmedEvent(
    Guid PaymentId,
    Guid UserId,
    PaymentPurpose Purpose,
    Guid? SubscriptionId,
    Guid? OrderId,
    decimal Amount,
    string Currency) : DomainEvent;