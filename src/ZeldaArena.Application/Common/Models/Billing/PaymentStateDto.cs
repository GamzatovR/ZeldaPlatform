using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Common.Models.Billing;

/// <summary>
/// Состояние платежа для страницы ввода кода (docs/SPEC.md §7.6, шаг 3): куда ушло
/// письмо, сколько осталось попыток и времени, и за что платят — от этого зависит,
/// куда страница отправит покупателя после оплаты или отмены.
///
/// Ни кода, ни его хеша здесь нет и быть не может — наружу они не выходят никогда.
/// </summary>
public sealed record PaymentStateDto(
    Guid PaymentId,
    PaymentStatus Status,
    string MaskedEmail,
    decimal Amount,
    string Currency,
    int AttemptsLeft,
    DateTimeOffset ConfirmationExpiresAt,
    bool CanResendNow,
    PaymentPurpose Purpose,
    string? OrderNumber);