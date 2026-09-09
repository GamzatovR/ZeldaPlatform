using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Application.Features.Payments;

/// <summary>
/// Когда код был выслан и можно ли выслать следующий (docs/SPEC.md §7.6, шаг 3).
///
/// Отдельного поля с моментом отправки в <c>Payments</c> нет (§6), и заводить его
/// не нужно: срок жизни кода фиксирован, поэтому момент выдачи однозначно
/// восстанавливается из <c>ConfirmationExpiresAt</c>. Расчёт собран здесь, потому
/// что им пользуются и сценарий повторной отправки, и запрос состояния — две копии
/// разъехались бы при первом изменении паузы.
/// </summary>
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