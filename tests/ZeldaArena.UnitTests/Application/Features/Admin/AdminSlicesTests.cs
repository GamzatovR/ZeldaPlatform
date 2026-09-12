using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Common.Models.Shop;
using ZeldaArena.Application.Features.Admin.Billing.Commands.CreatePlan;
using ZeldaArena.Application.Features.Admin.Billing.Commands.DeleteFeature;
using ZeldaArena.Application.Features.Admin.Billing.Commands.DeletePlan;
using ZeldaArena.Application.Features.Admin.Orders.Commands.ChangeOrderStatus;
using ZeldaArena.Application.Features.Admin.Players.Commands.DeletePlayer;
using ZeldaArena.Application.Features.Admin.Shop.Commands.CreateProduct;
using ZeldaArena.Application.Features.Admin.Shop.Commands.DeleteCategory;
using ZeldaArena.Application.Features.Admin.Shop.Commands.DeleteProduct;
using ZeldaArena.Application.Features.Admin.Users.Commands.SetUserBlocked;
using ZeldaArena.Application.Features.Admin.Users.Commands.SetUserRoles;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.Shop;
using ZeldaArena.Domain.ValueObjects;
using ZeldaArena.UnitTests.Application.Features.Shop;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Admin;

/// <summary>
/// Правила админских сценариев Фазы 9, которые нельзя проверить одним взглядом
/// на код: что удаляется, кого нельзя разжаловать и что происходит с деньгами
/// при отмене оплаченного заказа (docs/adr/ADR-0010).
/// </summary>
public class AdminSlicesTests
{
    private static readonly DateTimeOffset Now = ShopWorld.Now;

    private readonly RecordingUnitOfWork _unitOfWork = new();
    private readonly InMemoryFileStorage _storage = new();
    private readonly RecordingEntitlementCacheInvalidator _cache = new();

    // ─── Тарифы и фичи (EP-3, EP-4) ──────────────────────────────────────────

    [Fact]
    public async Task Plan_code_is_not_taken_twice()
    {
        var plans = new InMemoryRepository<Plan>(Plan.Create(PlanCodes.ProMonth, "Pro", Money.FromRubles(299m), 30));

        var result = await new CreatePlanCommandHandler(
                plans, new InMemoryReadRepository<Plan>(plans.Entities), new InMemoryQueryExecutor(), _unitOfWork)
            .Handle(new CreatePlanCommand(PlanCodes.ProMonth, "Другой", null, 1m, 30, 0), CancellationToken.None);

        result.Error.ShouldBe(BillingErrors.PlanCodeTaken);
    }

    [Fact]
    public async Task Paid_plan_without_duration_is_refused_by_the_entity()
    {
        var plans = new InMemoryRepository<Plan>();

        var result = await new CreatePlanCommandHandler(
                plans, new InMemoryReadRepository<Plan>(plans.Entities), new InMemoryQueryExecutor(), _unitOfWork)
            .Handle(new CreatePlanCommand("analytics", "Analytics", null, 500m, 0, 0), CancellationToken.None);

        result.Error.Code.ShouldBe("plan.paid_without_duration");
        plans.Entities.ShouldBeEmpty();
    }

    [Fact]
    public async Task Plan_with_subscriptions_is_not_deleted()
    {
        var plan = Plan.Create(PlanCodes.ProMonth, "Pro", Money.FromRubles(299m), 30);
        var plans = new InMemoryRepository<Plan>(plan);
        var subscription = Subscription.Activate(Guid.CreateVersion7(), plan, Now.AddDays(-400));

        var result = await new DeletePlanCommandHandler(
                plans, new InMemoryReadRepository<Subscription>([subscription]), new InMemoryQueryExecutor(), _cache, _unitOfWork)
            .Handle(new DeletePlanCommand(plan.Id), CancellationToken.None);

        result.Error.ShouldBe(BillingErrors.PlanHasSubscriptions);
        plans.Entities.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Feature_the_code_relies_on_is_not_deleted()
    {
        var feature = Feature.Create(FeatureCodes.TeamCreate, "Своя команда");
        var features = new InMemoryRepository<Feature>(feature);

        var result = await new DeleteFeatureCommandHandler(
                features, new InMemoryReadRepository<Plan>([]), new InMemoryQueryExecutor(), _cache, _unitOfWork)
            .Handle(new DeleteFeatureCommand(feature.Id), CancellationToken.None);

        result.Error.ShouldBe(BillingErrors.FeatureReferencedByCode);
        features.Entities.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Feature_inside_a_plan_is_not_deleted_but_a_free_one_is()
    {
        var feature = Feature.Create("news.early", "Ранний доступ к новостям");
        var plan = Plan.Create(PlanCodes.ProMonth, "Pro", Money.FromRubles(299m), 30);
        plan.GrantFeature(feature.Id);
        var features = new InMemoryRepository<Feature>(feature);
        var handler = new DeleteFeatureCommandHandler(
            features, new InMemoryReadRepository<Plan>([plan]), new InMemoryQueryExecutor(), _cache, _unitOfWork);

        var refused = await handler.Handle(new DeleteFeatureCommand(feature.Id), CancellationToken.None);
        refused.Error.ShouldBe(BillingErrors.FeatureInUse);

        plan.RevokeFeature(feature.Id);
        var deleted = await handler.Handle(new DeleteFeatureCommand(feature.Id), CancellationToken.None);

        deleted.IsSuccess.ShouldBeTrue();
        features.Entities.ShouldBeEmpty();
        _cache.InvalidatedEverything.ShouldBe(1, "права подписчиков меняются — кэш сбрасывается");
    }

    // ─── Пользователи ────────────────────────────────────────────────────────

    [Fact]
    public async Task Administrator_cannot_block_himself()
    {
        var me = Guid.CreateVersion7();
        var users = new InMemoryUserAdministrationService();

        var result = await new SetUserBlockedCommandHandler(users, new StubCurrentUserService { UserId = me })
            .Handle(new SetUserBlockedCommand(me, true), CancellationToken.None);

        result.Error.ShouldBe(AccountErrors.CannotBlockSelf);
        users.Blocked.ShouldBeEmpty();
    }

    [Fact]
    public async Task Last_administrator_keeps_the_role()
    {
        var users = new InMemoryUserAdministrationService();
        users.RoleCounts[RoleNames.Admin] = 1;
        var accounts = new InMemoryUserAccountService();
        var target = accounts.Add("admin@zeldaarena.local").Id;
        await accounts.AddToRoleAsync(target, RoleNames.Admin);

        var result = await new SetUserRolesCommandHandler(users, accounts, new StubCurrentUserService())
            .Handle(new SetUserRolesCommand(target, [RoleNames.User]), CancellationToken.None);

        result.Error.ShouldBe(AccountErrors.LastAdministrator);
        users.SavedRoles.ShouldBeEmpty();
    }

    [Fact]
    public async Task Premium_is_neither_granted_nor_taken_away_by_hand()
    {
        var users = new InMemoryUserAdministrationService();
        var accounts = new InMemoryUserAccountService();
        var target = accounts.Add("subscriber@zeldaarena.local").Id;
        await accounts.AddToRoleAsync(target, RoleNames.Premium);

        var result = await new SetUserRolesCommandHandler(users, accounts, new StubCurrentUserService())
            .Handle(new SetUserRolesCommand(target, [RoleNames.User, RoleNames.Moderator]), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        users.SavedRoles[target].ShouldBe([RoleNames.User, RoleNames.Moderator, RoleNames.Premium], ignoreOrder: true);
    }

    // ─── Магазин ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Product_sku_is_not_taken_twice()
    {
        var world = new ShopWorld();
        var existing = world.AddProduct("Hyrule Keyboard", 5000m, 3, world.Keyboards);

        var result = await new CreateProductCommandHandler(
                world.Products, world.ReadProducts(), world.ReadCategories(), new InMemoryQueryExecutor(), _storage, world.UnitOfWork)
            .Handle(
                new CreateProductCommand
                {
                    Sku = existing.Sku,
                    Name = "Другая клавиатура",
                    CategoryId = world.Keyboards.Id,
                    Price = 100m,
                    StockQuantity = 1,
                },
                CancellationToken.None);

        result.Error.ShouldBe(ShopErrors.SkuTaken);
    }

    [Fact]
    public async Task Category_with_products_is_not_deleted()
    {
        var world = new ShopWorld();
        world.AddProduct("Hyrule Keyboard", 5000m, 3, world.Keyboards);
        var categories = new InMemoryRepository<ProductCategory>(world.Keyboards);

        var result = await new DeleteCategoryCommandHandler(
                categories, world.ReadProducts(), new InMemoryQueryExecutor(), world.UnitOfWork)
            .Handle(new DeleteCategoryCommand(world.Keyboards.Id), CancellationToken.None);

        result.Error.ShouldBe(ShopErrors.CategoryNotEmpty);
    }

    [Fact]
    public async Task Product_that_was_never_ordered_is_deleted()
    {
        var world = new ShopWorld();
        var product = world.AddProduct("Hyrule Keyboard", 5000m, 3, world.Keyboards);

        var result = await new DeleteProductCommandHandler(
                world.Products, new InMemoryReadRepository<Order>([]), new InMemoryQueryExecutor(), _storage, world.UnitOfWork)
            .Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        world.Products.Entities.ShouldBeEmpty();
    }

    // ─── Игроки ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Player_who_played_for_a_team_is_not_deleted()
    {
        var world = new EsportsWorld();
        var team = world.AddTeam("Hyrule Knights", "HYR");
        var player = world.Sign(team, "Link", EsportsWorld.Now.AddYears(-1));
        var players = new InMemoryRepository<Player>(player);

        var result = await new DeletePlayerCommandHandler(
                players,
                world.Read(world.RosterEntries),
                world.Read(world.Matches),
                new InMemoryQueryExecutor(),
                _storage,
                _unitOfWork)
            .Handle(new DeletePlayerCommand(player.Id), CancellationToken.None);

        result.Error.Code.ShouldBe("player.has_history");
        players.Entities.ShouldHaveSingleItem();
    }

    // ─── Заказы ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Canceling_a_paid_order_returns_the_stock_and_refunds_the_payment()
    {
        var fixture = new OrderScenarioFixture();
        var product = fixture.World.AddProduct("Hyrule Keyboard", 5000m, stock: 10);
        var order = await fixture.BuyAsync(product, quantity: 2);
        var stockAfterPurchase = product.StockQuantity;

        var result = await fixture.ChangeStatusAsync(order.Number, OrderTransition.Cancel);

        result.IsSuccess.ShouldBeTrue();
        order.Status.ShouldBe(OrderStatus.Canceled);
        product.StockQuantity.ShouldBe(stockAfterPurchase + 2, "остаток вернулся на склад");
        fixture.SinglePayment.Status.ShouldBe(PaymentStatus.Refunded, "деньги вернулись покупателю");
        fixture.World.CartItems.ShouldBeEmpty("заказ отменил не покупатель — товары в корзину не возвращаются");
    }

    [Fact]
    public async Task Shipped_order_is_not_canceled()
    {
        var fixture = new OrderScenarioFixture();
        var order = await fixture.BuyAsync(fixture.World.AddProduct("Hyrule Keyboard", 5000m));
        (await fixture.ChangeStatusAsync(order.Number, OrderTransition.Ship)).IsSuccess.ShouldBeTrue();

        var result = await fixture.ChangeStatusAsync(order.Number, OrderTransition.Cancel);

        result.Error.Code.ShouldBe("order.cannot_cancel");
        order.Status.ShouldBe(OrderStatus.Shipped);
    }

    [Fact]
    public async Task Order_goes_from_paid_to_shipped_to_completed()
    {
        var fixture = new OrderScenarioFixture();
        var order = await fixture.BuyAsync(fixture.World.AddProduct("Hyrule Keyboard", 5000m));

        (await fixture.ChangeStatusAsync(order.Number, OrderTransition.Ship)).IsSuccess.ShouldBeTrue();
        (await fixture.ChangeStatusAsync(order.Number, OrderTransition.Complete)).IsSuccess.ShouldBeTrue();

        order.Status.ShouldBe(OrderStatus.Completed);
    }

    [Fact]
    public async Task Pending_order_cannot_be_shipped()
    {
        var fixture = new OrderScenarioFixture();
        var product = fixture.World.AddProduct("Hyrule Keyboard", 5000m);
        (await fixture.AddToCartAsync(product)).IsSuccess.ShouldBeTrue();
        (await fixture.PlaceAsync()).IsSuccess.ShouldBeTrue();

        var result = await fixture.ChangeStatusAsync(fixture.SingleOrder.Number, OrderTransition.Ship);

        result.Error.Code.ShouldBe("order.cannot_ship");
    }
}