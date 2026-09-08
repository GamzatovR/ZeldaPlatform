using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account;

/// <summary>
/// Вход временно заблокирован после пяти неудачных попыток (docs/SPEC.md §8.2).
/// Точное время разблокировки не показывается: оно подтвердило бы, что такой
/// пользователь существует.
/// </summary>
[AllowAnonymous]
public sealed class LockoutModel : PageModel
{
    public void OnGet()
    {
    }
}