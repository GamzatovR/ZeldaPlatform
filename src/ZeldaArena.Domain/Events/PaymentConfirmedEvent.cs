using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Domain.Events;

/// <summary>
/// Оплата подтверждена кодом из письма. Ни номера карты, ни CVV, ни самого кода
/// в событии нет и быть не может (docs/SPEC.md §7.6).
/// </summary>
public sealed record PaymentConfirmedEvent(
    Guid PaymentId,
    Guid UserId,
    PaymentPurpose Purpose,
    Guid? SubscriptionId,
    Guid? OrderId,
    decimal Amount,
    string Currency) : DomainEvent;