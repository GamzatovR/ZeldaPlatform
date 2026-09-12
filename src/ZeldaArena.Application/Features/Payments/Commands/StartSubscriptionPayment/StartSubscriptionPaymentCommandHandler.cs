using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Payments.Commands.StartSubscriptionPayment;

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

        // Заявка на подписку заводится до оплаты.
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