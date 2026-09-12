namespace ZeldaArena.Domain.Constants;

/// <summary>Языки интерфейса.</summary>
public static class SupportedCultures
{
    public const string Russian = "ru";

    public const string English = "en";

    public const string Default = Russian;

    public static IReadOnlyList<string> All { get; } = [Russian, English];

    public static bool IsSupported(string? cultureCode) =>
        cultureCode is not null
        && All.Contains(cultureCode, StringComparer.OrdinalIgnoreCase);
}