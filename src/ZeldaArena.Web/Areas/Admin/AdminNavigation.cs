using ZeldaArena.Web.Areas.Admin.Models;
using ZeldaArena.Web.Authorization;
using ZeldaArena.Web.Constants;

namespace ZeldaArena.Web.Areas.Admin;

/// <summary>
/// Боковое меню админки (docs/SPEC.md §9.4: «единый _AdminLayout с боковым меню»).
///
/// Пунктов ровно столько, сколько есть разделов: ссылка на контроллер, которого ещё нет,
/// роняет отрисовку всей страницы (урок Фазы 3, docs/PROGRESS.md). Каждый раздел
/// Фазы 9 добавляет свой пункт вместе со своим контроллером.
/// </summary>
public static class AdminNavigation
{
    public static IReadOnlyList<AdminNavItem> Items { get; } =
    [
        new("admin.nav.dashboard", "Dashboard", IconNames.Grid, PolicyNames.ModeratorOrAdmin),
        new("admin.nav.tournaments", "Tournaments", IconNames.Trophy, PolicyNames.CanManageCatalog),
        new("admin.nav.matches", "Matches", IconNames.Controller, PolicyNames.CanManageCatalog),
        new("admin.nav.teams", "Teams", IconNames.Users, PolicyNames.CanManageCatalog),
        new("admin.nav.players", "Players", IconNames.User, PolicyNames.CanManageCatalog),
        new("admin.nav.plans", "Plans", IconNames.Shield, PolicyNames.CanManageBilling),
        new("admin.nav.features", "Features", IconNames.Check, PolicyNames.CanManageBilling),
        new("admin.nav.products", "Products", IconNames.Cart, PolicyNames.AdminOnly),
        new("admin.nav.orders", "Orders", IconNames.Calendar, PolicyNames.AdminOnly),
        new("admin.nav.users", "Users", IconNames.Users, PolicyNames.AdminOnly),
    ];
}