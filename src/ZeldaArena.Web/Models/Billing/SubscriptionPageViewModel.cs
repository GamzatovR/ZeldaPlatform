using ZeldaArena.Application.Features.Subscriptions.Queries.GetMySubscription;
using ZeldaArena.Application.Features.Subscriptions.Queries.GetSubscriptionPlans;

namespace ZeldaArena.Web.Models.Billing;

/// <summary>
/// Страница тарифов и текущей подписки (docs/SPEC.md §9.3, п. 17).
///
/// <see cref="RequiredFeature"/> приходит из адреса, куда уводит отказ фича-гейта:
/// страница объясняет, какой функции не хватило и каким тарифом она открывается (§7.3).
/// </summary>
public sealed class SubscriptionPageViewModel
{
    public IReadOnlyList<PlanDto> Plans { get; init; } = [];

    public MySubscriptionDto? Current { get; init; }

    public string? RequiredFeature { get; init; }

    public string? ReturnUrl { get; init; }

    /// <summary>Тарифы, которыми открывается недостающая функция, — их и подсвечивает страница.</summary>
    public IReadOnlyList<Guid> PlansGrantingRequiredFeature =>
        RequiredFeature is null
            ? []
            : [.. Plans
                .Where(plan => plan.Features.Any(feature =>
                    string.Equals(feature.Code, RequiredFeature, StringComparison.OrdinalIgnoreCase)))
                .Select(plan => plan.Id)];
}