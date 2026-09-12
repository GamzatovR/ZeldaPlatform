using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Domain.Constants;
using ZeldaArena.Web.Models.Layout;

namespace ZeldaArena.Web.ViewComponents;

/// <summary>Шапка сайта.</summary>
public sealed class HeaderViewComponent : ViewComponent
{
    private static readonly IReadOnlyList<HeaderViewModel.NavItem> Items =
    [
        new("nav.home", "Home", "Index"),
        new("nav.tournaments", "Tournaments", "Index"),
        new("nav.schedule", "Schedule", "Index"),
        new("nav.teams", "Teams", "Index"),
        new("nav.players", "Players", "Index"),
        new("nav.shop", "Shop", "Index"),
        new("nav.subscription", "Subscription", "Index"),
    ];

    public IViewComponentResult Invoke()
    {
        var user = UserClaimsPrincipal;

        var model = new HeaderViewModel
        {
            NavItems = Items,
            IsAuthenticated = user.Identity?.IsAuthenticated == true,
            DisplayName = user.Identity?.Name,
            ShowPremiumBadge = user.IsInRole(RoleNames.Premium),
        };

        return View(model);
    }
}