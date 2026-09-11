using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Payments.Commands.StartSubscriptionPayment;

/// <summary>
/// Заводит платёж за подписку и высылает код (docs/SPEC.md §7.6).
///
/// Сумма берётся у тарифа, а не из запроса: цену считает сервер, значения с клиента
/// не принимаются (§15). По той же причине здесь нет ни поля суммы, ни поля валюты.
/// Общие шаги — идемпотентность, карта, код, письмо — у <see cref="PaymentInitiator"/>.
/// </summary>
public sealed class StartSubscriptionPaymentCommandHandler(
    ICurrentUserService currentUser,
    IReadRepository<Plan> plans,
    IRepository<Subscription> subscriptions,
    PaymentInitiator initiator,
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

        var existing = await initiator.FindExistingAsync(userId, request, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            return Result.Success(existing);
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

        var authorization = await initiator.AuthorizeAsync(request, plan.Price, cancellationToken)
            .ConfigureAwait(false);

        if (authorization.IsFailure)
        {
            return Result.Failure<StartPaymentResult>(authorization.Error);
        }

        // Заявка на подписку заводится до оплаты: платёж хранит сумму и ссылку
        // на подписку, но не тариф (§6), а при подтверждении кода тариф нужен —
        // у него берутся срок и цена. Прав такая запись не даёт: она в статусе
        // Pending. Если оплата не состоится, её удалит тот же сценарий, который
        // объявит платёж неудачным.
        var reserved = Subscription.Reserve(userId, plan, clock.UtcNow);

        await subscriptions.AddAsync(reserved, cancellationToken).ConfigureAwait(false);

        var initiated = await initiator
            .OpenAsync(
                userId,
                PaymentPurpose.Subscription,
                plan.Price,
                authorization.Value,
                request,
                cancellationToken,
                subscriptionId: reserved.Id)
            .ConfigureAwait(false);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await initiator.SendCodeAsync(initiated, currentUser.UserName, cancellationToken).ConfigureAwait(false);

        return Result.Success(initiated.ToResult());
    }
}