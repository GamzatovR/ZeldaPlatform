using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Web.Extensions;

namespace ZeldaArena.Web.Payments;

/// <summary>
/// Исход ввода кода: следующий шаг и сообщение для покупателя (<c>null</c> — сообщать
/// нечего). Правило одно на два входа — форму <c>PaymentController</c> и
/// <c>POST /api/payments/{id}/confirm</c>, иначе исход из docs/adr/ADR-0009 раздвоился бы.
/// </summary>
public sealed record PaymentConfirmationOutcome(PaymentNextStep Step, string? Message, bool IsError)
{
    private static readonly PaymentConfirmationOutcome StayOnForm = new(PaymentNextStep.StayOnForm, null, false);

    /// <summary>
    /// <paramref name="state"/> — состояние платежа после попытки, <paramref name="result"/> —
    /// исход команды подтверждения (<c>null</c>, если форма не прошла проверку модели).
    ///
    /// «Товары снова в корзине» говорится, только когда этот самый запрос провалил оплату
    /// (истёкший код, последняя попытка) и тем отменил заказ. Повторная отправка уже
    /// обработанного кода — кнопкой «Назад» после оплаты или после отмены — ведёт
    /// на страницу заказа, где виден его настоящий статус.
    /// </summary>
    public static PaymentConfirmationOutcome Resolve(PaymentStateDto state, Result? result, IStringLocalizer localizer)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(localizer);

        if (result is { IsSuccess: true } && state.Purpose != PaymentPurpose.Order)
        {
            return new(PaymentNextStep.SubscriptionActivated, null, false);
        }

        if (state.Purpose != PaymentPurpose.Order || state.OrderNumber is null)
        {
            return StayOnForm;
        }

        if (result is { IsSuccess: true } || state.Status == PaymentStatus.Succeeded)
        {
            return new(PaymentNextStep.OrderDetails, localizer["order.paid"].Value, false);
        }

        if (result is { IsFailure: true }
            && (result.Error.Code == BillingErrors.CodeExpired.Code || result.Error.Code == BillingErrors.NoAttemptsLeft.Code))
        {
            return new(
                PaymentNextStep.Cart,
                localizer.ForError(result.Error) + " " + localizer["order.items_returned"].Value,
                true);
        }

        if (state.Status != PaymentStatus.Pending)
        {
            var message = result is { IsFailure: true }
                ? localizer.ForError(result.Error)
                : localizer[BillingErrors.PaymentNotPending.Code].Value;

            return new(PaymentNextStep.OrderDetails, message, true);
        }

        return StayOnForm;
    }

    /// <summary>Сообщение в формате <c>_StatusMessage</c>: ошибка помечается ведущим «!».</summary>
    public string? StatusMessage => Message is null ? null : (IsError ? "!" : string.Empty) + Message;
}