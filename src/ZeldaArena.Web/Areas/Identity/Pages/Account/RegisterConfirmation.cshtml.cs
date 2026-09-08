using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account;

/// <summary>
/// «Проверьте почту» после регистрации. Ссылку подтверждения страница не показывает
/// даже в разработке: письмо целиком видно в MailHog на localhost:8025,
/// и отдельный обходной путь мимо почты только маскировал бы поломку отправки.
/// </summary>
[AllowAnonymous]
public sealed class RegisterConfirmationModel : PageModel
{
    public string? Email { get; private set; }

    public void OnGet(string? email) => Email = email;
}