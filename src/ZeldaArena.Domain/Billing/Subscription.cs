using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Events;

namespace ZeldaArena.Domain.Billing;

/// <summary>Подписка пользователя на тариф.</summary>
public class Subscription : BaseEntity, IAuditableEntity
{
    private Subscription()
    {
    }

    public Guid UserId { get; private set; }

    public Guid PlanId { get; private set; }

    public DateTimeOffset StartsAt { get; private set; }

    public DateTimeOffset EndsAt { get; private set; }

    public SubscriptionStatus Status { get; private set; }

    public bool AutoRenew { get; private set; }

    public decimal PriceSnapshot { get; private set; }

    public DateTimeOffset? CanceledAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public Plan? Plan { get; private set; }

    /// <summary>Единственный критерий доступа к платным функциям вместе с набором фич тарифа.</summary>
    public bool IsActiveAt(DateTimeOffset moment) =>
        Status == SubscriptionStatus.Active && EndsAt > moment;

    /// <summary>Создаёт активную подписку после подтверждённой оплаты.</summary>
    public static Subscription Activate(Guid userId, Plan plan, DateTimeOffset startsAt, bool autoRenew = false)
    {
        ArgumentNullException.ThrowIfNull(plan);

        InvariantViolationException.ThrowIf(
            userId == Guid.Empty,
            "subscription.user_required",
            "У подписки обязан быть пользователь.");

        InvariantViolationException.ThrowIf(
            plan.DurationDays < 1,
            "subscription.plan_without_duration",
            "Подписку нельзя оформить на тариф без срока действия.");

        var subscription = new Subscription
        {
            UserId = userId,
            PlanId = plan.Id,
            StartsAt = startsAt,
            EndsAt = startsAt.AddDays(plan.DurationDays),
            Status = SubscriptionStatus.Active,
            AutoRenew = autoRenew,
            PriceSnapshot = plan.Price.Amount,
        };

        subscription.Raise(new SubscriptionActivatedEvent(
            subscription.Id,
            subscription.UserId,
            subscription.PlanId,
            subscription.EndsAt));

        return subscription;
    }

    /// <summary>Заявка на подписку.</summary>
    public static Subscription Reserve(Guid userId, Plan plan, DateTimeOffset reservedAt)
    {
        ArgumentNullException.ThrowIfNull(plan);

        InvariantViolationException.ThrowIf(
            userId == Guid.Empty,
            "subscription.user_required",
            "У подписки обязан быть пользователь.");

        InvariantViolationException.ThrowIf(
            plan.DurationDays < 1,
            "subscription.plan_without_duration",
            "Подписку нельзя оформить на тариф без срока действия.");

        return new Subscription
        {
            UserId = userId,
            PlanId = plan.Id,
            StartsAt = reservedAt,
            EndsAt = reservedAt,
            Status = SubscriptionStatus.Pending,
            AutoRenew = false,
            PriceSnapshot = plan.Price.Amount,
        };
    }

    /// <summary>Продление действующей подписки.</summary>
    public void Extend(Plan plan, DateTimeOffset paidAt)
    {
        ArgumentNullException.ThrowIfNull(plan);

        InvariantViolationException.ThrowIf(
            Status == SubscriptionStatus.Canceled,
            "subscription.canceled_cannot_extend",
            "Отменённую подписку продлить нельзя — нужна новая.");

        InvariantViolationException.ThrowIf(
            plan.DurationDays < 1,
            "subscription.plan_without_duration",
            "Продлить можно только на тариф со сроком действия.");

        var from = EndsAt > paidAt ? EndsAt : paidAt;

        PlanId = plan.Id;
        EndsAt = from.AddDays(plan.DurationDays);
        PriceSnapshot = plan.Price.Amount;
        Status = SubscriptionStatus.Active;
        CanceledAt = null;

        Raise(new SubscriptionActivatedEvent(Id, UserId, PlanId, EndsAt));
    }

    /// <summary>Отказ от автопродления.</summary>
    public void Cancel(DateTimeOffset canceledAt)
    {
        InvariantViolationException.ThrowIf(
            Status != SubscriptionStatus.Active,
            "subscription.not_active",
            $"Отменить можно только активную подписку, текущий статус — {Status}.");

        AutoRenew = false;
        CanceledAt = canceledAt;
    }

    /// <summary>Немедленный отзыв подписки администратором.</summary>
    public void Terminate(DateTimeOffset terminatedAt)
    {
        InvariantViolationException.ThrowIf(
            Status is SubscriptionStatus.Canceled or SubscriptionStatus.Expired,
            "subscription.already_closed",
            $"Подписка уже закрыта, текущий статус — {Status}.");

        Status = SubscriptionStatus.Canceled;
        AutoRenew = false;
        CanceledAt = terminatedAt;
        EndsAt = terminatedAt;

        Raise(new SubscriptionExpiredEvent(Id, UserId, PlanId));
    }

    /// <summary>Помечает подписку истёкшей.</summary>
    public void Expire(DateTimeOffset moment)
    {
        InvariantViolationException.ThrowIf(
            Status != SubscriptionStatus.Active,
            "subscription.not_active",
            $"Истечь может только активная подписка, текущий статус — {Status}.");

        InvariantViolationException.ThrowIf(
            EndsAt > moment,
            "subscription.not_expired_yet",
            "Срок подписки ещё не закончился.");

        Status = SubscriptionStatus.Expired;

        Raise(new SubscriptionExpiredEvent(Id, UserId, PlanId));
    }

    public void SetAutoRenew(bool autoRenew)
    {
        InvariantViolationException.ThrowIf(
            Status != SubscriptionStatus.Active,
            "subscription.not_active",
            $"Автопродление настраивается только у активной подписки, текущий статус — {Status}.");

        AutoRenew = autoRenew;
    }
}