namespace ZeldaArena.Web.Models.Layout;

/// <summary>
/// Данные шапки. Собирается <c>HeaderViewComponent</c>; вьюха ничего не вычисляет.
/// </summary>
public sealed class HeaderViewModel
{
    public required IReadOnlyList<NavItem> NavItems { get; init; }

    public required bool IsAuthenticated { get; init; }

    public string? DisplayName { get; init; }

    /// <summary>
    /// Бейдж подписчика. Это <b>только отображение</b>: доступ к платным функциям
    /// даёт <c>IEntitlementService</c>, роль Premium для проверки прав не годится
    /// (docs/SPEC.md §7.4, §20 пункт 2).
    /// </summary>
    public required bool ShowPremiumBadge { get; init; }

    /// <summary>Пункт главного меню. Адрес задаётся маршрутом, а не строкой пути.</summary>
    public sealed record NavItem(string ResourceKey, string Controller, string Action);
}
