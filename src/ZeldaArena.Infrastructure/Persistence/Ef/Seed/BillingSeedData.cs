using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Seed;

public static class BillingSeedData
{
    public static IReadOnlyList<Feature> Features() =>
    [
        Feature.Create(
            FeatureCodes.TeamCreate,
            "Своя команда",
            "Создание команды, управление её профилем и составом."),
        Feature.Create(
            FeatureCodes.StatsAdvanced,
            "Расширенная статистика",
            "Графики формы и подробная разбивка показателей команд и игроков."),
    ];

    public static IReadOnlyList<Plan> Plans() =>
    [
        Plan.Create(
            PlanCodes.Free,
            "Бесплатный",
            Money.Zero(),
            durationDays: 0,
            "Просмотр турниров, матчей, команд и магазина.",
            sortOrder: 0),
        Plan.Create(
            PlanCodes.ProMonth,
            "Pro на месяц",
            new Money(299m, Money.DefaultCurrency),
            durationDays: 30,
            "Своя команда и расширенная статистика на 30 дней.",
            sortOrder: 1),
        Plan.Create(
            PlanCodes.ProYear,
            "Pro на год",
            new Money(2490m, Money.DefaultCurrency),
            durationDays: 365,
            "То же самое на год и почти на треть дешевле.",
            sortOrder: 2),
    ];

    /// <summary>Какие фичи входят в тариф. Бесплатный тариф не даёт ничего.</summary>
    public static IReadOnlyDictionary<string, string[]> PlanFeatures() =>
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            [PlanCodes.Free] = [],
            [PlanCodes.ProMonth] = [FeatureCodes.TeamCreate, FeatureCodes.StatsAdvanced],
            [PlanCodes.ProYear] = [FeatureCodes.TeamCreate, FeatureCodes.StatsAdvanced],
        };
}