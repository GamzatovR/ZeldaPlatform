using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Domain.Constants;
using ZeldaArena.Web.Models.Layout;

namespace ZeldaArena.Web.ViewComponents;

/// <summary>
/// Шапка сайта (docs/SPEC.md §10.2): меню, поиск, бейдж подписки, меню аккаунта.
///
/// Пунктов меню ровно столько, сколько сейчас есть маршрутов. Ссылка на страницу,
/// которой ещё нет, — это не пустая ссылка, а сломанный раздел, и в Фазе 3 такая
/// ссылка уже роняла страницу целиком. Каждая следующая фаза добавляет свои пункты
/// вместе со своими контроллерами:
///
/// <list type="bullet">
///   <item>Фаза 6 — Турниры, Расписание, Команды, Игроки (отдельной ленты новостей нет:
///   новость открывается из карточки на главной);</item>
///   <item>Фаза 7 — Магазин;</item>
///   <item>Фаза 8 — счётчик корзины, поиск через AJAX;</item>
///   <item>Фаза 10 — колокольчик уведомлений;</item>
///   <item>Фаза 11 — переключатель языка (место под него — в подвале).</item>
/// </list>
/// </summary>
public sealed class HeaderViewComponent : ViewComponent
{
    private static readonly IReadOnlyList<HeaderViewModel.NavItem> Items =
    [
        new("nav.home", "Home", "Index"),
        new("nav.tournaments", "Tournaments", "Index"),
        new("nav.schedule", "Schedule", "Index"),
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