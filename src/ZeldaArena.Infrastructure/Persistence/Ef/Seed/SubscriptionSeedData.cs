namespace ZeldaArena.Infrastructure.Persistence.Ef.Seed;

public static class SubscriptionSeedData
{
    public const string SubscriberEmail = "subscriber@zeldaarena.local";

    public const string ExpiredEmail = "expired@zeldaarena.local";

    public static IReadOnlyList<string> DemoEmails { get; } = [SubscriberEmail, ExpiredEmail];

    public static IReadOnlyList<(string Email, int StartedDaysAgo)> Subscriptions { get; } =
    [
        (SubscriberEmail, 5),
        (ExpiredEmail, 60),
    ];
}