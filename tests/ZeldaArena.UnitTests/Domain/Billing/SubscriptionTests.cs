using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Events;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.UnitTests.Domain.Billing;

public class SubscriptionTests
{
    private static readonly DateTimeOffset Now = new(2026, 3, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Activation_takes_duration_from_the_plan_and_raises_an_event()
    {
        var plan = ProMonth();

        var subscription = Subscription.Activate(Guid.CreateVersion7(), plan, Now);

        subscription.Status.ShouldBe(SubscriptionStatus.Active);
        subscription.EndsAt.ShouldBe(Now.AddDays(30));
        subscription.PriceSnapshot.ShouldBe(299m);
        subscription.DomainEvents.OfType<SubscriptionActivatedEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Access_is_granted_only_while_active_and_not_expired()
    {
        var subscription = Subscription.Activate(Guid.CreateVersion7(), ProMonth(), Now);

        subscription.IsActiveAt(Now.AddDays(29)).ShouldBeTrue();
        subscription.IsActiveAt(Now.AddDays(31)).ShouldBeFalse();
    }

    [Fact]
    public void Free_plan_cannot_be_subscribed_to()
    {
        var free = Plan.Create(PlanCodes.Free, "Free", Money.Zero(), durationDays: 0);

        Should.Throw<InvariantViolationException>(
                () => Subscription.Activate(Guid.CreateVersion7(), free, Now))
            .Code.ShouldBe("subscription.plan_without_duration");
    }

    [Fact]
    public void Renewal_adds_days_to_the_paid_period_not_to_today()
    {
        // Иначе пользователь, продливший заранее, терял бы оплаченные дни.
        var subscription = Subscription.Activate(Guid.CreateVersion7(), ProMonth(), Now);

        subscription.Extend(ProMonth(), Now.AddDays(10));

        subscription.EndsAt.ShouldBe(Now.AddDays(60));
    }

    [Fact]
    public void Expired_subscription_is_renewed_from_the_payment_date()
    {
        var subscription = Subscription.Activate(Guid.CreateVersion7(), ProMonth(), Now);
        var paidAt = Now.AddDays(45);

        subscription.Extend(ProMonth(), paidAt);

        subscription.EndsAt.ShouldBe(paidAt.AddDays(30));
    }

    [Fact]
    public void Cancellation_only_disables_auto_renew_and_keeps_access()
    {
        //.5, п. 5: доступ сохраняется до конца оплаченного срока.
        var subscription = Subscription.Activate(Guid.CreateVersion7(), ProMonth(), Now, autoRenew: true);

        subscription.Cancel(Now.AddDays(3));

        subscription.AutoRenew.ShouldBeFalse();
        subscription.CanceledAt.ShouldBe(Now.AddDays(3));
        subscription.Status.ShouldBe(SubscriptionStatus.Active);
        subscription.IsActiveAt(Now.AddDays(20)).ShouldBeTrue();
    }

    [Fact]
    public void Termination_by_admin_revokes_access_immediately()
    {
        var subscription = Subscription.Activate(Guid.CreateVersion7(), ProMonth(), Now);
        subscription.ClearDomainEvents();

        subscription.Terminate(Now.AddDays(3));

        subscription.Status.ShouldBe(SubscriptionStatus.Canceled);
        subscription.IsActiveAt(Now.AddDays(4)).ShouldBeFalse();
        subscription.DomainEvents.OfType<SubscriptionExpiredEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Expiration_is_only_possible_after_the_end_date()
    {
        var subscription = Subscription.Activate(Guid.CreateVersion7(), ProMonth(), Now);

        Should.Throw<InvariantViolationException>(() => subscription.Expire(Now.AddDays(10)))
            .Code.ShouldBe("subscription.not_expired_yet");

        subscription.Expire(Now.AddDays(31));

        subscription.Status.ShouldBe(SubscriptionStatus.Expired);
        subscription.DomainEvents.OfType<SubscriptionExpiredEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Terminated_subscription_cannot_be_extended()
    {
        var subscription = Subscription.Activate(Guid.CreateVersion7(), ProMonth(), Now);
        subscription.Terminate(Now.AddDays(1));

        Should.Throw<InvariantViolationException>(() => subscription.Extend(ProMonth(), Now.AddDays(2)))
            .Code.ShouldBe("subscription.canceled_cannot_extend");
    }

    private static Plan ProMonth() =>
        Plan.Create(PlanCodes.ProMonth, "Pro на месяц", new Money(299m, "RUB"), durationDays: 30);
}