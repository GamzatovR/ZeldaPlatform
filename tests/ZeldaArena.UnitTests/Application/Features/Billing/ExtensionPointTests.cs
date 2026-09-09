using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Features.Admin.Billing.Commands.CreateFeature;
using ZeldaArena.Application.Features.Admin.Billing.Commands.SetPlanFeatures;
using ZeldaArena.Application.Features.Admin.Billing.Commands.ToggleFeature;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.ValueObjects;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Billing;

/// <summary>
/// Точки расширения подписок (docs/SPEC.md §5.4) — то, что прямо оценивается
/// на защите: EP-3 (новая платная функция), EP-4 (вынос функции в отдельную
/// услугу) и EP-5 (параметр функции).
///
/// Проверяется не только запись в базу, но и сброс кэша прав: без него доступ
/// у пользователей менялся бы «когда-нибудь в течение пяти минут», и демонстрация
/// «без деплоя» не состоялась бы.
/// </summary>
public class ExtensionPointTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 9, 12, 0, 0, TimeSpan.Zero);

    private readonly RecordingEntitlementCacheInvalidator _cache = new();

    private readonly Feature _teamCreate = Feature.Create(FeatureCodes.TeamCreate, "Своя команда");
    private readonly Feature _statsAdvanced = Feature.Create(FeatureCodes.StatsAdvanced, "Статистика");

    /// <summary>
    /// Полная демонстрация EP-4 из чек-листа §19: администратор снимает
    /// <c>stats.advanced</c> с тарифа Pro и заводит для неё отдельный тариф —
    /// доступ у пользователей меняется без единой строки кода.
    /// </summary>
    [Fact]
    public async Task A_feature_can_be_taken_out_of_a_plan_and_sold_separately()
    {
        var pro = PlanWith("pro-month", _teamCreate, _statsAdvanced);
        var analytics = PlanWith("analytics");

        var user = Guid.CreateVersion7();
        var proSubscription = Subscription.Activate(user, pro, Now.AddDays(-1));

        Entitlements(proSubscription, plans: [pro, analytics])
            .Has(FeatureCodes.StatsAdvanced)
            .ShouldBeTrue();

        // Шаг 1. Снимаем расширенную статистику с тарифа Pro.
        (await SetFeaturesAsync(pro, [_teamCreate])).IsSuccess.ShouldBeTrue();

        Entitlements(proSubscription, plans: [pro, analytics])
            .Has(FeatureCodes.StatsAdvanced)
            .ShouldBeFalse();

        // Право на свою команду при этом никуда не делось.
        Entitlements(proSubscription, plans: [pro, analytics])
            .Has(FeatureCodes.TeamCreate)
            .ShouldBeTrue();

        // Шаг 2. Продаём её отдельным тарифом — и тот, кто его купил, получает доступ.
        (await SetFeaturesAsync(analytics, [_statsAdvanced])).IsSuccess.ShouldBeTrue();

        var analyticsSubscription = Subscription.Activate(user, analytics, Now);

        Entitlements(proSubscription, analyticsSubscription, plans: [pro, analytics])
            .Has(FeatureCodes.StatsAdvanced)
            .ShouldBeTrue();
    }

    /// <summary>Без сброса кэша правка тарифа дошла бы до пользователей только через пять минут.</summary>
    [Fact]
    public async Task Rewriting_a_plan_drops_the_cached_entitlements_for_everyone()
    {
        var pro = PlanWith("pro-month", _teamCreate);

        await SetFeaturesAsync(pro, [_teamCreate, _statsAdvanced]);

        _cache.InvalidatedEverything.ShouldBe(1);
    }

    /// <summary>EP-5: параметр фичи задаётся из админки и доезжает до прав пользователя.</summary>
    [Fact]
    public async Task A_feature_parameter_reaches_the_entitlements()
    {
        var pro = PlanWith("pro-month");

        await SetFeaturesAsync(pro, [(_teamCreate, "3")]);

        var subscription = Subscription.Activate(Guid.CreateVersion7(), pro, Now.AddDays(-1));

        Entitlements(subscription, plans: [pro]).ValueOf(FeatureCodes.TeamCreate).ShouldBe("3");
    }

    [Fact]
    public async Task A_parameter_can_be_changed_without_duplicating_the_grant()
    {
        var pro = PlanWith("pro-month");

        await SetFeaturesAsync(pro, [(_teamCreate, "1")]);
        await SetFeaturesAsync(pro, [(_teamCreate, "5")]);

        pro.PlanFeatures.ShouldHaveSingleItem().Value.ShouldBe("5");
    }

    /// <summary>
    /// Повторная выдача той же фичи не должна плодить вторую привязку: у PlanFeatures
    /// составной первичный ключ, и вторая строка — это падение на дубле ключа.
    ///
    /// На живом приложении так и случилось: репозиторий отдавал тариф с пустым составом,
    /// потому что коллекция — навигация, а GetByIdAsync её не грузил. Лечится
    /// AutoInclude в PlanConfiguration; здесь проверяется само правило домена.
    /// </summary>
    [Fact]
    public async Task Granting_the_same_feature_twice_does_not_duplicate_the_row()
    {
        var pro = PlanWith("pro-month", _teamCreate);

        await SetFeaturesAsync(pro, [_teamCreate, _statsAdvanced]);
        await SetFeaturesAsync(pro, [_teamCreate, _statsAdvanced]);

        pro.PlanFeatures.Count.ShouldBe(2);
        pro.PlanFeatures.Select(planFeature => planFeature.FeatureId).Distinct().Count().ShouldBe(2);
    }

    /// <summary>Привязка к несуществующей фиче молча не давала бы никаких прав — это ошибка формы.</summary>
    [Fact]
    public async Task An_unknown_feature_is_refused()
    {
        var pro = PlanWith("pro-month", _teamCreate);

        var result = await Handler(pro).Handle(
            new SetPlanFeaturesCommand(pro.Id, [new PlanFeatureAssignment(Guid.CreateVersion7(), null)]),
            CancellationToken.None);

        result.Error.ShouldBe(BillingErrors.FeatureNotFound);

        // Прежний состав не тронут, кэш не сброшен: неудачная команда ничего не меняет.
        pro.PlanFeatures.ShouldHaveSingleItem().FeatureId.ShouldBe(_teamCreate.Id);
        _cache.InvalidatedEverything.ShouldBe(0);
    }

    /// <summary>EP-3: новая платная функция — это строка в таблице, а не правка кода.</summary>
    [Fact]
    public async Task A_new_feature_is_just_a_row()
    {
        var features = new InMemoryRepository<Feature>();

        var result = await new CreateFeatureCommandHandler(
                features,
                new InMemoryReadRepository<Feature>(features.Entities),
                new InMemoryQueryExecutor(),
                new RecordingUnitOfWork())
            .Handle(
                new CreateFeatureCommand("Match.Predictions", "Прогнозы", null),
                CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();

        // Код приводится к нижнему регистру: имя политики Feature:{code} обязано
        // совпадать с тем, что напишут в атрибуте.
        features.Entities.ShouldHaveSingleItem().Code.ShouldBe("match.predictions");
    }

    [Fact]
    public async Task A_duplicate_feature_code_is_refused()
    {
        var features = new InMemoryRepository<Feature>(_teamCreate);

        var result = await new CreateFeatureCommandHandler(
                features,
                new InMemoryReadRepository<Feature>(features.Entities),
                new InMemoryQueryExecutor(),
                new RecordingUnitOfWork())
            .Handle(
                new CreateFeatureCommand(FeatureCodes.TeamCreate, "Ещё одна команда", null),
                CancellationToken.None);

        result.Error.ShouldBe(BillingErrors.FeatureCodeTaken);
        features.Entities.ShouldHaveSingleItem();
    }

    /// <summary>
    /// Выключение фичи — самый быстрый способ закрыть функцию всем сразу,
    /// не разбирая тарифы.
    /// </summary>
    [Fact]
    public async Task Switching_a_feature_off_closes_it_for_everyone_at_once()
    {
        var pro = PlanWith("pro-month", _teamCreate);
        var subscription = Subscription.Activate(Guid.CreateVersion7(), pro, Now.AddDays(-1));

        var result = await new ToggleFeatureCommandHandler(
                new InMemoryRepository<Feature>(_teamCreate, _statsAdvanced),
                _cache,
                new RecordingUnitOfWork())
            .Handle(new ToggleFeatureCommand(_teamCreate.Id, IsActive: false), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        _cache.InvalidatedEverything.ShouldBe(1);

        Entitlements(subscription, plans: [pro]).Features.ShouldBeEmpty();
    }

    private Plan PlanWith(string code, params Feature[] features)
    {
        var plan = Plan.Create(code, code, new Money(299m, Money.DefaultCurrency), durationDays: 30);

        foreach (var feature in features)
        {
            plan.GrantFeature(feature.Id);
        }

        return plan;
    }

    private Task<Result> SetFeaturesAsync(Plan plan, IEnumerable<Feature> features) =>
        SetFeaturesAsync(plan, features.Select(feature => (feature, (string?)null)));

    private Task<Result> SetFeaturesAsync(
        Plan plan,
        IEnumerable<(Feature Feature, string? Value)> assignments) =>
        Handler(plan).Handle(
            new SetPlanFeaturesCommand(
                plan.Id,
                [.. assignments.Select(item => new PlanFeatureAssignment(item.Feature.Id, item.Value))]),
            CancellationToken.None);

    private SetPlanFeaturesCommandHandler Handler(params Plan[] plans) =>
        new(
            new InMemoryRepository<Plan>(plans),
            new InMemoryReadRepository<Feature>([_teamCreate, _statsAdvanced]),
            new InMemoryQueryExecutor(),
            _cache,
            new RecordingUnitOfWork());

    /// <summary>
    /// Права считаются тем же кодом, что и в рабочем приложении: смысл проверки
    /// в том, что правка тарифа доходит до реального разрешения доступа.
    /// </summary>
    private EntitlementSet Entitlements(Subscription subscription, Plan[] plans) =>
        Entitlements([subscription], plans);

    private EntitlementSet Entitlements(Subscription first, Subscription second, Plan[] plans) =>
        Entitlements([first, second], plans);

    private EntitlementSet Entitlements(Subscription[] subscriptions, Plan[] plans) =>
        EntitlementResolver.Resolve(
            subscriptions,
            plans.ToDictionary(plan => plan.Id),
            new[] { _teamCreate, _statsAdvanced }.ToDictionary(feature => feature.Id),
            Now);
}