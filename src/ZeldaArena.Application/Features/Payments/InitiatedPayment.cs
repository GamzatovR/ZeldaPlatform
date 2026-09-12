using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Domain.Billing;

namespace ZeldaArena.Application.Features.Payments;

public sealed record InitiatedPayment(Payment Payment, ConfirmationCode Code)
{
    public StartPaymentResult ToResult() =>
        new(Payment.Id, MaskedEmail.Of(Payment.ConfirmationEmail));
}