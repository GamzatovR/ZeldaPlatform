using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Domain.Billing;

namespace ZeldaArena.Application.Features.Payments;

/// <summary>
/// Только что заведённый платёж вместе с сырым кодом. Код нужен ровно до письма
/// (<see cref="PaymentInitiator.SendCodeAsync"/>) и дальше не уходит: в базе лежит хеш.
/// </summary>
public sealed record InitiatedPayment(Payment Payment, ConfirmationCode Code)
{
    public StartPaymentResult ToResult() =>
        new(Payment.Id, MaskedEmail.Of(Payment.ConfirmationEmail));
}