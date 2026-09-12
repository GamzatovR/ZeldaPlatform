namespace ZeldaArena.Web.Models.Layout;

public sealed class HeaderViewModel
{
    public required IReadOnlyList<NavItem> NavItems { get; init; }

    public required bool IsAuthenticated { get; init; }

    public string? DisplayName { get; init; }

    public required bool ShowPremiumBadge { get; init; }

    /// <summary>Пункт главного меню. Адрес задаётся маршрутом, а не строкой пути.</summary>
    public sealed record NavItem(string ResourceKey, string Controller, string Action);
}