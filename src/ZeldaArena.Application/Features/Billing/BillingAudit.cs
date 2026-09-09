namespace ZeldaArena.Application.Features.Billing;

/// <summary>
/// Типы сущностей для записей аудита по сценариям биллинга (docs/SPEC.md §13).
/// Покупка подписки, оформление и отмена — в списке обязательных к записи событий §8.2.
///
/// Константы, а не литералы: по этому полю в админке фильтруется журнал, и одной
/// опечатки достаточно, чтобы часть записей выпала из выборки.
/// </summary>
public static class BillingAudit
{
    public const string Payment = nameof(Domain.Billing.Payment);

    public const string Subscription = nameof(Domain.Billing.Subscription);

    public const string Plan = nameof(Domain.Billing.Plan);

    public const string Feature = nameof(Domain.Billing.Feature);
}