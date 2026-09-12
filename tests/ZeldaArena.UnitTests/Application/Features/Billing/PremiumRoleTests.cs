using ZeldaArena.Application.Common.Events;
using ZeldaArena.Application.Features.Subscriptions.EventHandlers;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Events;
using ZeldaArena.Domain.ValueObjects;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Billing;

public class PremiumRoleTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 9, 12, 0, 0, TimeSpan.Zero);

    private readonly InMemoryUserAccountService _accounts = new();
    private readonly RecordingEntitlementCacheInvalidator _cache = new();

    [Fact]
    public async Task Activation_grants_the_premium_badge()
    {
        var user = _accounts.Add("player@zeldaarena.test");

        await ActivatedAsync(user.Id);

        var updated = await _accounts.FindByIdAsync(user.Id);
        updated.ShouldNotBeNull().Roles.ShouldContain(RoleNames.Premium);
    }

    [Fact]
    public async Task Activation_drops_the_cached_entitlements()
    {
        var user = _accounts.Add("player@zeldaarena.test");

        await ActivatedAsync(user.Id);

        _cache.InvalidatedUsers.ShouldBe([user.Id]);
    }

    [Fact]
    public async Task A_missing_account_does_not_break_the_activation()
    {
        var logger = new RecordingLogger<SubscriptionActivatedEventHandler>();
        var unknown = Guid.CreateVersion7();

        await new SubscriptionActivatedEventHandler(_accounts, _cache, logger).Handle(
            new DomainEventNotification<SubscriptionActivatedEvent>(
                new SubscriptionActivatedEvent(Guid.CreateVersion7(), unknown, Guid.CreateVersion7(), Now)),
            CancellationToken.None);

        logger.Messages.ShouldNotBeEmpty();
        _cache.InvalidatedUsers.ShouldBe([unknown]);
    }

    [Fact]
    public async Task Expiry_takes_the_premium_badge_away()
    {
        var user = _accounts.Add("player@zeldaarena.test");
        await ActivatedAsync(user.Id);

        await ExpiredAsync(user.Id, Guid.CreateVersion7());

        var updated = await _accounts.FindByIdAsync(user.Id);
        updated.ShouldNotBeNull().Roles.ShouldNotContain(RoleNames.Premium);
    }

    [Fact]
    public async Task The_badge_survives_while_another_subscription_is_still_running()
    {
        var user = _accounts.Add("player@zeldaarena.test");
        await ActivatedAsync(user.Id);

        var plan = Plan.Create("analytics", "Аналитика", new Money(199m, Money.DefaultCurrency), 30);
        var stillRunning = Subscription.Activate(user.Id, plan, Now);

        await ExpiredAsync(user.Id, Guid.CreateVersion7(), stillRunning);

        var updated = await _accounts.FindByIdAsync(user.Id);
        updated.ShouldNotBeNull().Roles.ShouldContain(RoleNames.Premium);
    }

    /// <summary>Кэш сбрасывается в любом случае: набор фич изменился, даже если бейдж остался.</summary>
    [Fact]
    public async Task Expiry_always_drops_the_cached_entitlements()
    {
        var user = _accounts.Add("player@zeldaarena.test");
        var plan = Plan.Create("analytics", "Аналитика", new Money(199m, Money.DefaultCurrency), 30);

        await ExpiredAsync(user.Id, Guid.CreateVersion7(), Subscription.Activate(user.Id, plan, Now));

        _cache.InvalidatedUsers.ShouldBe([user.Id]);
    }

    private Task ActivatedAsync(Guid userId) =>
        new SubscriptionActivatedEventHandler(
            _accounts,
            _cache,
            new RecordingLogger<SubscriptionActivatedEventHandler>())
        .Handle(
            new DomainEventNotification<SubscriptionActivatedEvent>(
                new SubscriptionActivatedEvent(
                    Guid.CreateVersion7(),
                    userId,
                    Guid.CreateVersion7(),
                    Now.AddDays(30))),
            CancellationToken.None);

    private Task ExpiredAsync(Guid userId, Guid subscriptionId, params Subscription[] others) =>
        new SubscriptionExpiredEventHandler(
            _accounts,
            _cache,
            new InMemoryReadRepository<Subscription>(others),
            new InMemoryQueryExecutor(),
            new FixedDateTimeProvider(Now),
            new RecordingLogger<SubscriptionExpiredEventHandler>())
        .Handle(
            new DomainEventNotification<SubscriptionExpiredEvent>(
                new SubscriptionExpiredEvent(subscriptionId, userId, Guid.CreateVersion7())),
            CancellationToken.None);
}