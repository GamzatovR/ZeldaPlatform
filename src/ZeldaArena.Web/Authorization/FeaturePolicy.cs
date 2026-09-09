namespace ZeldaArena.Web.Authorization;

/// <summary>
/// Имена динамических политик платных функций (docs/SPEC.md §7.3, уровень 2).
///
/// Политика <c>Feature:team.create</c> нигде не регистрируется заранее: её собирает
/// на лету <see cref="FeaturePolicyProvider"/>. Иначе добавление платной функции
/// строкой в таблицу Features требовало бы правки кода — ровно того, чего EP-3
/// не должно требовать.
/// </summary>
public static class FeaturePolicy
{
    public const string Prefix = "Feature:";

    /// <summary>Куда уводить того, у кого функции нет: не голый 403, а страница тарифов.</summary>
    public const string SubscriptionPath = "/account/subscription";

    /// <summary>
    /// Параметр запроса с кодом недостающей функции. Страница тарифов по нему
    /// объясняет, что именно и каким тарифом открывается (§7.3).
    /// </summary>
    public const string RequiredFeatureParameter = "required";

    public static string NameFor(string featureCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureCode);

        return Prefix + featureCode.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Возвращает код функции, если имя политики относится к платным функциям,
    /// иначе <c>null</c> — тогда имя разбирает провайдер политик по умолчанию.
    /// </summary>
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