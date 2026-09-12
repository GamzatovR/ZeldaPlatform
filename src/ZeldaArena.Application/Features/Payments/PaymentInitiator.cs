using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Payments;

public sealed class PaymentInitiator(
    IRepository<Payment> payments,
    IReadRepository<Payment> paymentsForRead,
    IQueryExecutor queryExecutor,
    IPaymentGateway gateway,
    IConfirmationCodeProtector codes,
    IBillingEmailSender emailSender,
    IDateTimeProvider clock)
{
    public async Task<StartPaymentResult?> FindExistingAsync(
        Guid userId,
        ICardPaymentDetails details,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(details);

        var idempotencyKey = PaymentIdempotency.KeyFor(userId, details.IdempotencyKey);

        var query = paymentsForRead.Query()
            .Where(payment => payment.IdempotencyKey == idempotencyKey)
            .Select(payment => new PaymentIdentity(payment.Id, payment.ConfirmationEmail));

        var existing = await queryExecutor.FirstOrDefaultAsync(query, cancellationToken).ConfigureAwait(false);

        return existing is null
            ? null
            : new StartPaymentResult(existing.PaymentId, MaskedEmail.Of(existing.ConfirmationEmail));
    }

    public Task<Result<CardAuthorization>> AuthorizeAsync(
        ICardPaymentDetails details,
        Money amount,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(details);

        return gateway.AuthorizeAsync(
            new CardPaymentRequest(
                details.CardNumber,
                details.ExpiryMonth,
                details.ExpiryYear,
                details.Cvv,
                amount),
            cancellationToken);
    }

    public async Task<InitiatedPayment> OpenAsync(
        Guid userId,
        PaymentPurpose purpose,
        Money amount,
        CardAuthorization authorization,
        ICardPaymentDetails details,
        CancellationToken cancellationToken,
        Guid? subscriptionId = null,
        Guid? orderId = null)
    {
        ArgumentNullException.ThrowIfNull(authorization);
        ArgumentNullException.ThrowIfNull(details);

        var code = codes.Issue();

        var payment = Payment.Start(
            userId,
            purpose,
            amount,
            authorization.CardLast4,
            authorization.CardBrand,
            details.ConfirmationEmail,
            code.Hash,
            clock.UtcNow.Add(PaymentPolicy.CodeLifetime),
            PaymentIdempotency.KeyFor(userId, details.IdempotencyKey),
            subscriptionId,
            orderId);

        await payments.AddAsync(payment, cancellationToken).ConfigureAwait(false);

        return new InitiatedPayment(payment, code);
    }

    public Task SendCodeAsync(
        InitiatedPayment initiated,
        string? displayName,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(initiated);

        return emailSender.SendPaymentCodeAsync(
            initiated.Payment.ConfirmationEmail,
            displayName,
            initiated.Code.Code,
            initiated.Payment.ConfirmationExpiresAt,
            cancellationToken);
    }

    private sealed record PaymentIdentity(Guid PaymentId, string ConfirmationEmail);
}