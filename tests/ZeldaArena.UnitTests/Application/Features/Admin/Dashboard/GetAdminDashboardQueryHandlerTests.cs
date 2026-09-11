using ZeldaArena.Application.Features.Admin.Dashboard.Queries.GetAdminDashboard;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Shop;
using ZeldaArena.Domain.ValueObjects;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Admin.Dashboard;

public class GetAdminDashboardQueryHandlerTests
{
    private const string CodeHash = "code-hash";

    private static readonly DateTimeOffset Now = EsportsWorld.Now;

    private readonly EsportsWorld _world = new();
    private readonly List<Subscription> _subscriptions = [];
    private readonly List<Payment> _payments = [];
    private readonly List<Order> _orders = [];
    private readonly InMemoryUserAdministrationService _users = new() { UserCount = 42 };
    private readonly StubCurrentUserService _currentUser = new() { UserId = Guid.CreateVersion7() };

    [Fact]
    public async Task Moderator_sees_the_sport_figures_but_no_business_block()
    {
        _currentUser.Roles.Add(RoleNames.Moderator);
        var teamA = _world.AddTeam("Hyrule", "HYR");
        var teamB = _world.AddTeam("Gerudo", "GRD");
        _world.Schedule(teamA, teamB, Now.AddHours(2)).Start(Now);
        _world.AddUserTeam(Guid.CreateVersion7(), "Kokiri", "KOK");
        AddPayment(500m, Now.AddDays(-1));

        var dashboard = await Handle();

        dashboard.LiveMatchCount.ShouldBe(1);
        dashboard.TeamsAwaitingApproval.ShouldBe(1);
        dashboard.Business.ShouldBeNull();
    }

    [Fact]
    public async Task Administrator_gets_users_subscriptions_revenue_and_orders()
    {
        _currentUser.Roles.Add(RoleNames.Admin);
        var plan = Plan.Create(PlanCodes.ProMonth, "Pro", Money.FromRubles(299m), 30);
        _subscriptions.Add(Subscription.Activate(Guid.CreateVersion7(), plan, Now.AddDays(-1)));
        _subscriptions.Add(Subscription.Activate(Guid.CreateVersion7(), plan, Now.AddDays(-60)));
        var paid = PlaceOrder("ZA-2026-000001", Now.AddHours(-2));
        paid.MarkPaid(Now.AddHours(-1));
        PlaceOrder("ZA-2026-000002", Now.AddHours(-1));

        var business = (await Handle()).Business.ShouldNotBeNull();

        business.UserCount.ShouldBe(42);
        business.ActiveSubscriptions.ShouldBe(1, "подписка, начатая 60 дней назад на 30 дней, уже истекла");
        business.OrdersToShip.ShouldBe(1);
        business.RecentOrders.Select(order => order.Number)
            .ShouldBe(["ZA-2026-000002", "ZA-2026-000001"], "последние заказы — от новых к старым");
    }

    [Fact]
    public async Task Revenue_counts_only_succeeded_payments_and_the_last_30_days_separately()
    {
        _currentUser.Roles.Add(RoleNames.Admin);
        AddPayment(299m, Now.AddDays(-3));
        AddPayment(2_490m, Now.AddDays(-45));
        StartPayment(1_000m);

        var business = (await Handle()).Business.ShouldNotBeNull();

        business.RevenueTotal.ShouldBe(2_789m, "неподтверждённый платёж в выручку не входит");
        business.RevenueLast30Days.ShouldBe(299m);
        business.Currency.ShouldBe(Money.DefaultCurrency);
    }

    [Fact]
    public async Task Empty_shop_gives_zero_revenue_rather_than_nothing()
    {
        _currentUser.Roles.Add(RoleNames.Admin);

        var business = (await Handle()).Business.ShouldNotBeNull();

        business.RevenueTotal.ShouldBe(0m);
        business.RecentOrders.ShouldBeEmpty();
    }

    [Fact]
    public async Task Upcoming_matches_put_live_first_and_skip_finished_ones()
    {
        var teamA = _world.AddTeam("Hyrule", "HYR");
        var teamB = _world.AddTeam("Gerudo", "GRD");
        var soon = _world.Schedule(teamA, teamB, Now.AddHours(1));
        var live = _world.Schedule(teamA, teamB, Now.AddHours(5));
        live.Start(Now);
        var finished = _world.Schedule(teamA, teamB, Now.AddHours(-5));
        finished.Start(Now.AddHours(-5));
        finished.UpdateScore(2, 0);
        finished.Finish(Now.AddHours(-4));

        var dashboard = await Handle();

        dashboard.UpcomingMatches.Select(match => match.Id).ShouldBe([live.Id, soon.Id]);
    }

    private Task<AdminDashboardDto> Handle() =>
        new GetAdminDashboardQueryHandler(
            _world.Read(_world.Matches),
            _world.Read(_world.Teams),
            new InMemoryReadRepository<Subscription>(_subscriptions),
            new InMemoryReadRepository<Payment>(_payments),
            new InMemoryReadRepository<Order>(_orders),
            _users,
            new InMemoryQueryExecutor(),
            _currentUser,
            new FixedDateTimeProvider(Now))
            .Handle(new GetAdminDashboardQuery(), CancellationToken.None);

    private void AddPayment(decimal amount, DateTimeOffset paidAt)
    {
        var payment = StartPayment(amount);
        payment.Confirm(CodeHash, paidAt).ShouldBe(PaymentConfirmationResult.Succeeded);
    }

    private Payment StartPayment(decimal amount)
    {
        var payment = Payment.Start(
            Guid.CreateVersion7(),
            PaymentPurpose.Subscription,
            Money.FromRubles(amount),
            "4242",
            "Visa",
            "buyer@zeldaarena.local",
            CodeHash,
            Now.AddYears(1),
            Guid.CreateVersion7().ToString());
        _payments.Add(payment);

        return payment;
    }

    private Order PlaceOrder(string number, DateTimeOffset placedAt)
    {
        var order = Order.Place(
            Guid.CreateVersion7(),
            number,
            new ShippingAddress("Линк", "+7 900 000-00-00", "RU", "Москва", "Тверская, 1", "101000"),
            [new OrderLine(Guid.CreateVersion7(), "Клавиатура Hyrule K1", 2000m, 1)],
            placedAt);
        _orders.Add(order);

        return order;
    }
}