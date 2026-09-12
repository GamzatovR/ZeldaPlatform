namespace ZeldaArena.Web.Authorization;

/// <summary>Имена динамических политик платных функций.</summary>
public static class FeaturePolicy
{
    public const string Prefix = "Feature:";

    /// <summary>Куда уводить того, у кого функции нет: не голый 403, а страница тарифов.</summary>
    public const string SubscriptionPath = "/account/subscription";

    public const string RequiredFeatureParameter = "required";

    public static string NameFor(string featureCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureCode);

        return Prefix + featureCode.Trim().ToLowerInvariant();
    }

    public static string? FeatureCodeOf(string policyName)
    {
        ArgumentNullException.ThrowIfNull(policyName);

        if (!policyName.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return null;
        }

        var code = policyName[Prefix.Length..];

        return code.Length == 0 ? null : code.ToLowerInvariant();
    }
}