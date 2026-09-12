namespace ZeldaArena.Application.Common.Models;

public sealed record EntitlementSet(IReadOnlyDictionary<string, string?> Features)
{
    public static readonly EntitlementSet Empty =
        new(new Dictionary<string, string?>(StringComparer.Ordinal));

    public bool Has(string featureCode) => Features.ContainsKey(featureCode);

    public string? ValueOf(string featureCode) =>
        Features.TryGetValue(featureCode, out var value) ? value : null;
}