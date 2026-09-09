using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Payments.Commands.StartSubscriptionPayment;

/// <summary>
/// Заводит платёж и высылает код (docs/SPEC.md §7.6).
///
/// Сумма берётся у тарифа, а не из запроса: цену считает сервер, значения с клиента
/// не принимаются (§15). По той же причине здесь нет ни поля суммы, ни поля валюты.
///
/// Письмо отправляется внутри транзакции команды — по тому же соображению, что
/// и при регистрации в Фазе 3: недоступный SMTP обязан откатить и создание платежа,
/// иначе пользователь получит запись в ожидании, подтвердить которую нечем.
/// </summary>
public sealed class StartSubscriptionPaymentCommandHandler(
    ICurrentUserService currentUser,
    IReadRepository<Plan> plans,
    IRepository<Payment> payments,
    IRepository<Subscription> subscriptions,
    IReadRepository<Payment> paymentsForRead,
    IQueryExecutor queryExecutor,
    IPaymentGateway gateway,
    IConfirmationCodeProtector codes,
    IBillingEmailSender emailSender,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock)
    : IRequestHandler<StartSubscriptionPaymentCommand, Result<StartPaymentResult>>
{
    public async Task<Result<StartPaymentResult>> Handle(
        StartSubscriptionPaymentCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure<StartPaymentResult>(AccountErrors.UserNotFound);
        }

        // Идемпотентность (§7.6): повторная отправка формы — обновлённая страница,
        // второй клик, возврат по «Назад» — не заводит второй платёж и не шлёт
        // второе письмо, а возвращает тот же самый. Владелец зашит в ключ, поэтому
        // чужой ключ не столкнётся с нашим на уникальном индексе.
        var idempotencyKey = PaymentIdempotency.KeyFor(userId, request.IdempotencyKey);

        var existing = await FindByIdempotencyKeyAsync(idempotencyKey, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            return Result.Success(new StartPaymentResult(
                existing.PaymentId,
                MaskedEmail.Of(existing.ConfirmationEmail)));
        }

        var plan = await plans.FindAsync(request.PlanId, cancellationToken).ConfigureAwait(false);

        if (plan is null)
        {
            return Result.Failure<StartPaymentResult>(BillingErrors.PlanNotFound);
        }

        if (!plan.IsActive)
        {
            return Result.Failure<StartPaymentResult>(BillingErrors.PlanInactive);
        }

        // Бесплатный тариф оплачивать нечем и незачем: у него нет ни цены, ни срока.
        if (plan.IsFree || plan.DurationDays < 1)
        {
            return Result.Failure<StartPaymentResult>(BillingErrors.PlanNotPurchasable);
        }

        var authorization = await gateway
            .AuthorizeAsync(
                new CardPaymentRequest(
                    request.CardNumber,
                    request.ExpiryMonth,
                    request.ExpiryYear,
                    request.Cvv,
                    plan.Price),
                cancellationToken)
            .ConfigureAwait(false);

        if (authorization.IsFailure)
        {
            return Result.Failure<StartPaymentResult>(authorization.Error);
        }

        var code = codes.Issue();
        var now = clock.UtcNow;

        // Заявка на подписку заводится до оплаты: платёж хранит сумму и ссылку
        // на подписку, но не тариф (§6), а при подтверждении кода тариф нужен —
        // у него берутся срок и цена. Прав такая запись не даёт: она в статусе
        // Pending. Если оплата не состоится, её удалит тот же сценарий, который
        // объявит платёж неудачным.
        var reserved = Subscription.Reserve(userId, plan, now);

        await subscriptions.AddAsync(reserved, cancellationToken).ConfigureAwait(false);

        var payment = Payment.Start(
            userId,
            PaymentPurpose.Subscription,
            plan.Price,
            authorization.Value.CardLast4,
            authorization.Value.CardBrand,
            request.ConfirmationEmail,
            code.Hash,
            now.Add(PaymentPolicy.CodeLifetime),
            idempotencyKey,
            subscriptionId: reserved.Id);

        await payments.AddAsync(payment, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await emailSender
            .SendPaymentCodeAsync(
                payment.ConfirmationEmail,
                currentUser.UserName,
                code.Code,
                payment.ConfirmationExpiresAt,
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(new StartPaymentResult(
            payment.Id,
            MaskedEmail.Of(payment.ConfirmationEmail)));
    }

    /// <summary>
    /// Владелец уже зашит в ключ (<see cref="PaymentIdempotency"/>), поэтому искать
    /// дополнительно по <c>UserId</c> не нужно: чужой платёж по такому ключу
    /// не найдётся никогда.
    /// </summary>
    private Task<PaymentIdentity?> FindByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var query = paymentsForRead.Query()
            .Where(payment => payment.IdempotencyKey == idempotencyKey)
            .Select(payment => new PaymentIdentity(payment.Id, payment.ConfirmationEmail));

        return queryExecutor.FirstOrDefaultAsync(query, cancellationToken);
    }

    private sealed record PaymentIdentity(Guid PaymentId, string ConfirmationEmail);
}