using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Features.Subscriptions.Commands.CancelSubscription;
using ZeldaArena.Application.Features.Subscriptions.Commands.ExpireDueSubscriptions;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Events;
using ZeldaArena.Domain.ValueObjects;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Billing;

public class SubscriptionLifecycleTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 9, 12, 0, 0, TimeSpan.Zero);

    private readonly Guid _userId = Guid.CreateVersion7();
    private readonly Plan _plan = Plan.Create("pro-month", "Pro", new Money(299m, Money.DefaultCurrency), 30);

    [Fact]
    public async Task Cancelling_keeps_the_access_until_the_period_ends()
    {
        var subscription = Subscription.Activate(_userId, _plan, Now.AddDays(-5), autoRenew: true);

        var result = await CancelAsync(subscription);

        result.IsSuccess.ShouldBeTrue();
        subscription.AutoRenew.ShouldBeFalse();
        subscription.CanceledAt.ShouldBe(Now);
        subscription.Status.ShouldBe(SubscriptionStatus.Active);
        subscription.IsActiveAt(Now).ShouldBeTrue();
    }

    [Fact]
    public async Task Cancelling_without_a_subscription_reports_it_plainly()
    {
        var result = await CancelAsync();

        result.Error.ShouldBe(BillingErrors.SubscriptionNotActive);
    }

    /// <summary>Чужую подписку отменить нельзя: сценарий работает только со своей.</summary>
    [Fact]
    public async Task Another_users_subscription_is_not_cancelled()
    {
        var foreign = Subscription.Activate(Guid.CreateVersion7(), _plan, Now.AddDays(-5));

        var result = await CancelAsync(foreign);

        result.Error.ShouldBe(BillingErrors.SubscriptionNotActive);
        foreign.CanceledAt.ShouldBeNull();
    }

    [Fact]
    public async Task Subscriptions_past_their_end_date_are_marked_expired()
    {
        var due = Subscription.Activate(_userId, _plan, Now.AddDays(-40));

        var result = await ExpireAsync(due);

        result.Value.ShouldBe(1);
        due.Status.ShouldBe(SubscriptionStatus.Expired);
    }

    /// <summary>Истечение поднимает событие — на нём держится снятие роли и письмо.</summary>
    [Fact]
    public async Task Expiry_raises_the_domain_event()
    {
        var due = Subscription.Activate(_userId, _plan, Now.AddDays(-40));
        due.ClearDomainEvents();

        await ExpireAsync(due);

        due.DomainEvents.OfType<SubscriptionExpiredEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public async Task A_running_subscription_is_left_alone()
    {
        var running = Subscription.Activate(_userId, _plan, Now.AddDays(-5));

        var result = await ExpireAsync(running);

        result.Value.ShouldBe(0);
        running.Status.ShouldBe(SubscriptionStatus.Active);
    }

    /// <summary>Отозванная администратором подписка второй раз не истекает.</summary>
    [Fact]
    public async Task An_already_closed_subscription_is_skipped()
    {
        var terminated = Subscription.Activate(_userId, _plan, Now.AddDays(-40));
        terminated.Terminate(Now.AddDays(-1));

        var result = await ExpireAsync(terminated);

        result.Value.ShouldBe(0);
    }

    [Fact]
    public async Task Nothing_due_means_nothing_done() =>
        (await ExpireAsync()).Value.ShouldBe(0);

    private Task<Result> CancelAsync(params Subscription[] subscriptions) =>
        new CancelSubscriptionCommandHandler(
            new StubCurrentUserService { UserId = _userId },
            new InMemoryRepository<Subscription>(subscriptions),
            new InMemoryReadRepository<Subscription>(subscriptions),
            new InMemoryQueryExecutor(),
            new RecordingUnitOfWork(),
            new FixedDateTimeProvider(Now))
        .Handle(new CancelSubscriptionCommand(), CancellationToken.None);

    private static Task<Result<int>> ExpireAsync(params Subscription[] subscriptions) =>
        new ExpireDueSubscriptionsCommandHandler(
            new InMemoryReadRepository<Subscription>(subscriptions),
            new InMemoryRepository<Subscription>(subscriptions),
            new InMemoryQueryExecutor(),
            new RecordingUnitOfWork(),
            new FixedDateTimeProvider(Now),
            new RecordingLogger<ExpireDueSubscriptionsCommandHandler>())
        .Handle(new ExpireDueSubscriptionsCommand(), CancellationToken.None);
}