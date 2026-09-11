using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Payments;

/// <summary>
/// Общий путь заведения мнимого платежа (docs/SPEC.md §7.6, шаг 2) для подписки
/// и заказа: идемпотентность, авторизация карты, код, запись платежа, письмо.
///
/// Разбит на шаги, а не собран в один метод, потому что между ними сценарии делают
/// своё: подписка заводит заявку на тариф, заказ списывает остаток и очищает корзину.
/// Порядок шагов при этом один: карта проверяется до того, как сценарий что-то
/// поменяет, — отклонённая карта не должна оставлять за собой ни заявки, ни заказа.
/// </summary>
public sealed class PaymentInitiator(
    IRepository<Payment> payments,
    IReadRepository<Payment> paymentsForRead,
    IQueryExecutor queryExecutor,
    IPaymentGateway gateway,
    IConfirmationCodeProtector codes,
    IBillingEmailSender emailSender,
    IDateTimeProvider clock)
{
    /// <summary>
    /// Идемпотентность (§7.6): повторная отправка формы — обновлённая страница,
    /// второй клик, возврат по «Назад» — не заводит второй платёж и не шлёт
    /// второе письмо, а возвращает тот же самый. Владелец зашит в ключ
    /// (<see cref="PaymentIdempotency"/>), поэтому чужой платёж по нему не найдётся.
    /// </summary>
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

    /// <summary>
    /// Выпускает код и заводит платёж в ожидании. От карты в платёж попадают только
    /// последние четыре цифры и платёжная система из <paramref name="authorization"/>.
    /// </summary>
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

    /// <summary>
    /// Письмо отправляется внутри транзакции команды — по тому же соображению, что
    /// и при регистрации в Фазе 3: недоступный SMTP обязан откатить и создание платежа,
    /// иначе пользователь получит запись в ожидании, подтвердить которую нечем.
    /// </summary>
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