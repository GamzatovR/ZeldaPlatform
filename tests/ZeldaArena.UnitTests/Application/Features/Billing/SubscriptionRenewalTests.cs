using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Events;

namespace ZeldaArena.UnitTests.Application.Features.Billing;

public class SubscriptionRenewalTests
{
    private readonly PaymentScenarioFixture _fixture = new();

    [Fact]
    public async Task Buying_a_longer_plan_adds_its_days_to_the_paid_period()
    {
        var monthly = await _fixture.PayAsync(idempotencyKey: "first");

        monthly.EndsAt.ShouldBe(PaymentScenarioFixture.Start.AddDays(30));

        _fixture.Advance(TimeSpan.FromDays(10));
        var upgraded = await _fixture.PayAsync(_fixture.Yearly.Id, "second");

        // Двадцать оплаченных дней месячного тарифа остались при пользователе.
        upgraded.EndsAt.ShouldBe(PaymentScenarioFixture.Start.AddDays(30).AddDays(365));
        upgraded.PlanId.ShouldBe(_fixture.Yearly.Id);
    }

    /// <summary>Двух подписок у одного человека быть не должно — заявка растворяется в продлении.</summary>
    [Fact]
    public async Task An_upgrade_leaves_exactly_one_subscription()
    {
        await _fixture.PayAsync(idempotencyKey: "first");
        await _fixture.PayAsync(_fixture.Yearly.Id, "second");

        _fixture.Subscriptions.Entities.ShouldHaveSingleItem();
    }

    /// <summary>Платёж-продление ссылается на ту подписку, которую он продлил, а не на исчезнувшую заявку.</summary>
    [Fact]
    public async Task The_renewal_payment_points_at_the_surviving_subscription()
    {
        await _fixture.PayAsync(idempotencyKey: "first");
        var subscription = await _fixture.PayAsync(_fixture.Yearly.Id, "second");

        _fixture.Payments.Entities[^1].SubscriptionId.ShouldBe(subscription.Id);
    }

    [Fact]
    public async Task Renewing_after_a_gap_counts_from_the_payment_date()
    {
        await _fixture.PayAsync(idempotencyKey: "first");

        _fixture.Advance(TimeSpan.FromDays(100));
        var renewed = await _fixture.PayAsync(idempotencyKey: "second");

        renewed.EndsAt.ShouldBe(PaymentScenarioFixture.Start.AddDays(100).AddDays(30));
        renewed.Status.ShouldBe(SubscriptionStatus.Active);
    }

    /// <summary>Продление поднимает событие: роль Premium и кэш прав обновляются и на нём.</summary>
    [Fact]
    public async Task Renewal_raises_the_activation_event_again()
    {
        var subscription = await _fixture.PayAsync(idempotencyKey: "first");
        subscription.ClearDomainEvents();

        await _fixture.PayAsync(_fixture.Yearly.Id, "second");

        subscription.DomainEvents
            .OfType<SubscriptionActivatedEvent>()
            .ShouldHaveSingleItem()
            .EndsAt.ShouldBe(subscription.EndsAt);
    }
}