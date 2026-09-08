using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account;

/// <summary>
/// Заглушка отказа в доступе. В Фазе 11 её место займёт стилизованная страница 403
/// с двумя вариантами — «не хватает прав» и «нужна подписка» (docs/SPEC.md §14.2).
/// </summary>
[AllowAnonymous]
public sealed class AccessDeniedModel : PageModel
{
    public void OnGet()
    {
    }
}