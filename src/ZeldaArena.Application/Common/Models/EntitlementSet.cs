namespace ZeldaArena.Application.Common.Models;

/// <summary>
/// Набор прав пользователя: код фичи → значение параметра фичи (docs/SPEC.md §7.3).
/// Значение непустое у параметризованных фич (EP-5), например лимит команд;
/// у обычных фич оно <c>null</c>, а важен сам факт наличия ключа.
///
/// Тариф в набор не входит намеренно: код никогда не спрашивает «какой у пользователя
/// тариф», только «есть ли фича» (§7.1).
/// </summary>
public sealed record EntitlementSet(IReadOnlyDictionary<string, string?> Features)
{
    public static readonly EntitlementSet Empty =
        new(new Dictionary<string, string?>(StringComparer.Ordinal));

    public bool Has(string featureCode) => Features.ContainsKey(featureCode);

    public string? ValueOf(string featureCode) =>
        Features.TryGetValue(featureCode, out var value) ? value : null;
}