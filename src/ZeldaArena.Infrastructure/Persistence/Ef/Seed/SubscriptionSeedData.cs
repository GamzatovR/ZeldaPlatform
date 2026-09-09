namespace ZeldaArena.Infrastructure.Persistence.Ef.Seed;

/// <summary>
/// Подписки демо-пользователей (docs/SPEC.md §6): активная, истёкшая и никакой.
///
/// Адреса совпадают с учётными записями из appsettings.Development.json — их заводит
/// IdentitySeeder. Третий демо-пользователь в списке отсутствует намеренно: без
/// подписки должно быть видно, как фича-гейт уводит на страницу тарифов.
/// </summary>
public static class SubscriptionSeedData
{
    public const string SubscriberEmail = "subscriber@zeldaarena.local";

    public const string ExpiredEmail = "expired@zeldaarena.local";

    public static IReadOnlyList<string> DemoEmails { get; } = [SubscriberEmail, ExpiredEmail];

    /// <summary>
    /// Сколько дней назад началась подписка. Тариф месячный, поэтому 5 дней назад —
    /// действующая, а 60 — давно закончившаяся.
    /// </summary>
    public static IReadOnlyList<(string Email, int StartedDaysAgo)> Subscriptions { get; } =
    [
        (SubscriberEmail, 5),
        (ExpiredEmail, 60),
    ];
}