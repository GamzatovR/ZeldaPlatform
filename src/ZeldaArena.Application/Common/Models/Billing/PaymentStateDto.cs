using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Common.Models.Billing;

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