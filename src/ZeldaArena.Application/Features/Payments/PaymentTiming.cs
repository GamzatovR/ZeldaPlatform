using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Application.Features.Payments;

public static class PaymentTiming
{
    public static DateTimeOffset IssuedAt(Payment payment)
    {
        ArgumentNullException.ThrowIfNull(payment);

        return payment.ConfirmationExpiresAt - PaymentPolicy.CodeLifetime;
    }

    public static DateTimeOffset ResendAllowedAt(Payment payment) =>
        IssuedAt(payment) + PaymentPolicy.ResendCooldown;

    public static bool CanResendAt(Payment payment, DateTimeOffset moment) =>
        moment >= ResendAllowedAt(payment);
}