using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Events;

namespace ZeldaArena.Domain.Billing;

/// <summary>
/// Подписка пользователя на тариф. Цена фиксируется снапшотом: подорожание тарифа
/// не должно менять историю уже оплаченных подписок (docs/SPEC.md §6).
/// </summary>
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

    /// <summary>
    /// Единственный критерий доступа к платным функциям вместе с набором фич тарифа:
    /// активный статус и незакончившийся срок (docs/SPEC.md §7.3).
    /// </summary>
    public bool IsActiveAt(DateTimeOffset moment) =>
        Status == SubscriptionStatus.Active && EndsAt > moment;

    /// <summary>
    /// Создаёт активную подписку после подтверждённой оплаты. Срок берётся у тарифа,
    /// а не приходит из запроса.
    /// </summary>
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

    /// <summary>
    /// Продление действующей подписки: срок прибавляется к текущему концу, а не к «сейчас»,
    /// иначе пользователь терял бы оплаченные дни. Истёкшая подписка продлевается от даты оплаты.
    /// </summary>
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

    /// <summary>
    /// Отказ от автопродления. Доступ сохраняется до конца оплаченного срока —
    /// решение зафиксировано в docs/SPEC.md §7.5, п. 5.
    /// </summary>
    public void Cancel(DateTimeOffset canceledAt)
    {
        InvariantViolationException.ThrowIf(
            Status != SubscriptionStatus.Active,
            "subscription.not_active",
            $"Отменить можно только активную подписку, текущий статус — {Status}.");

        AutoRenew = false;
        CanceledAt = canceledAt;
    }

    /// <summary>
    /// Немедленный отзыв подписки администратором: доступ пропадает сразу.
    /// В отличие от <see cref="Cancel"/> оплаченные дни не сохраняются.
    /// </summary>
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

    /// <summary>
    /// Помечает подписку истёкшей. Вызывается фоновой службой раз в час (docs/SPEC.md §7.5, п. 4).
    /// </summary>
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