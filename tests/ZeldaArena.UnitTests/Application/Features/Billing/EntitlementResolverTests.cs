using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.UnitTests.Application.Features.Billing;

/// <summary>
/// Права на платные функции — единственное, что решает доступ (docs/SPEC.md §7.1),
/// поэтому правила отбора проверяются подробно: ошибка здесь либо закрывает
/// оплаченную функцию, либо раздаёт её даром.
/// </summary>
public class EntitlementResolverTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 9, 12, 0, 0, TimeSpan.Zero);

    private readonly Feature _teamCreate = Feature.Create(FeatureCodes.TeamCreate, "Своя команда");
    private readonly Feature _statsAdvanced = Feature.Create(FeatureCodes.StatsAdvanced, "Статистика");

    [Fact]
    public void Active_subscription_grants_every_feature_of_its_plan()
    {
        var plan = ProPlan(_teamCreate, _statsAdvanced);
        var subscription = Subscription.Activate(Guid.CreateVersion7(), plan, Now.AddDays(-1));

        var entitlements = Resolve([subscription], [plan]);

        entitlements.Has(FeatureCodes.TeamCreate).ShouldBeTrue();
        entitlements.Has(FeatureCodes.StatsAdvanced).ShouldBeTrue();
    }

    [Fact]
    public void Expired_subscription_grants_nothing()
    {
        var plan = ProPlan(_teamCreate);
        var subscription = Subscription.Activate(Guid.CreateVersion7(), plan, Now.AddDays(-90));
        subscription.Expire(Now);

        Resolve([subscription], [plan]).Features.ShouldBeEmpty();
    }

    /// <summary>
    /// Срок кончился, но фоновая служба ещё не добралась до записи. Доступ обязан
    /// пропасть по дате, а не по расторопности планировщика (§7.5, п. 4).
    /// </summary>
    [Fact]
    public void Subscription_past_its_end_date_grants_nothing_even_while_still_marked_active()
    {
        var plan = ProPlan(_teamCreate);
        var subscription = Subscription.Activate(Guid.CreateVersion7(), plan, Now.AddDays(-40));

        Resolve([subscription], [plan], Now.AddDays(1)).Features.ShouldBeEmpty();
    }

    /// <summary>
    /// Отказ от автопродления сохраняет доступ до конца оплаченного срока —
    /// решение зафиксировано в §7.5, п. 5.
    /// </summary>
    [Fact]
    public void Canceled_auto_renew_keeps_access_until_the_paid_period_ends()
    {
        var plan = ProPlan(_teamCreate);
        var subscription = Subscription.Activate(Guid.CreateVersion7(), plan, Now.AddDays(-1));
        subscription.Cancel(Now);

        Resolve([subscription], [plan]).Has(FeatureCodes.TeamCreate).ShouldBeTrue();
    }

    /// <summary>Отзыв администратором, в отличие от отказа от продления, закрывает доступ сразу.</summary>
    [Fact]
    public void Terminated_subscription_grants_nothing()
    {
        var plan = ProPlan(_teamCreate);
        var subscription = Subscription.Activate(Guid.CreateVersion7(), plan, Now.AddDays(-1));
        subscription.Terminate(Now);

        Resolve([subscription], [plan]).Features.ShouldBeEmpty();
    }

    [Fact]
    public void Features_of_several_active_subscriptions_are_united()
    {
        var teams = ProPlan(_teamCreate);
        var stats = ProPlan(_statsAdvanced);
        var userId = Guid.CreateVersion7();

        var entitlements = Resolve(
            [
                Subscription.Activate(userId, teams, Now.AddDays(-1)),
                Subscription.Activate(userId, stats, Now.AddDays(-1)),
            ],
            [teams, stats]);

        entitlements.Features.Count.ShouldBe(2);
    }

    /// <summary>
    /// Прямая проверка EP-3: администратор выключает фичу, и доступ пропадает
    /// у всех, кому её давал тариф, — без правки тарифов и без деплоя.
    /// </summary>
    [Fact]
    public void Deactivated_feature_stops_granting_access()
    {
        var plan = ProPlan(_teamCreate);
        var subscription = Subscription.Activate(Guid.CreateVersion7(), plan, Now.AddDays(-1));

        _teamCreate.Deactivate();

        Resolve([subscription], [plan]).Features.ShouldBeEmpty();
    }

    /// <summary>
    /// Тариф, снятый с продажи, не отбирает права у тех, кто уже заплатил:
    /// IsActive у плана означает «продаётся», а не «действует».
    /// </summary>
    [Fact]
    public void Plan_withdrawn_from_sale_keeps_serving_those_who_already_paid()
    {
        var plan = ProPlan(_teamCreate);
        var subscription = Subscription.Activate(Guid.CreateVersion7(), plan, Now.AddDays(-1));

        plan.Deactivate();

        Resolve([subscription], [plan]).Has(FeatureCodes.TeamCreate).ShouldBeTrue();
    }

    /// <summary>Параметр фичи — это EP-5: лимит или процент, задаваемый из админки.</summary>
    [Fact]
    public void Parameterised_feature_carries_its_value()
    {
        var plan = ProPlan();
        plan.GrantFeature(_teamCreate.Id, "3");
        var subscription = Subscription.Activate(Guid.CreateVersion7(), plan, Now.AddDays(-1));

        Resolve([subscription], [plan]).ValueOf(FeatureCodes.TeamCreate).ShouldBe("3");
    }

    /// <summary>
    /// Две подписки дают одну фичу с разными параметрами. Побеждает та, что действует
    /// дольше: иначе значение лимита зависело бы от порядка строк в выборке.
    /// </summary>
    [Fact]
    public void Longer_running_subscription_decides_the_feature_value()
    {
        var userId = Guid.CreateVersion7();

        var monthly = ProPlan();
        monthly.GrantFeature(_teamCreate.Id, "1");

        var yearly = ProPlan(durationDays: 365);
        yearly.GrantFeature(_teamCreate.Id, "5");

        var shorter = Subscription.Activate(userId, monthly, Now);
        var longer = Subscription.Activate(userId, yearly, Now);

        Resolve([shorter, longer], [monthly, yearly])
            .ValueOf(FeatureCodes.TeamCreate)
            .ShouldBe("5");

        Resolve([longer, shorter], [monthly, yearly])
            .ValueOf(FeatureCodes.TeamCreate)
            .ShouldBe("5");
    }

    [Fact]
    public void User_without_subscriptions_has_no_features()
    {
        Resolve([], []).ShouldBe(EntitlementSet.Empty);
    }

    private Plan ProPlan(params Feature[] features)
    {
        var plan = ProPlan(durationDays: 30);

        foreach (var feature in features)
        {
            plan.GrantFeature(feature.Id);
        }

        return plan;
    }

    private static Plan ProPlan(int durationDays) =>
        Plan.Create(
            $"pro-{Guid.CreateVersion7():N}",
            "Pro",
            new Money(299m, Money.DefaultCurrency),
            durationDays);

    private EntitlementSet Resolve(
        Subscription[] subscriptions,
        Plan[] plans,
        DateTimeOffset? moment = null) =>
        EntitlementResolver.Resolve(
            subscriptions,
            plans.ToDictionary(plan => plan.Id),
            new[] { _teamCreate, _statsAdvanced }.ToDictionary(feature => feature.Id),
            moment ?? Now);
}