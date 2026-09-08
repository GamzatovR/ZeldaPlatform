using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using ZeldaArena.Application.Features.Account.Commands.SignOut;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account;

/// <summary>
/// Выход. Только POST и только с антифоржери-токеном: по ссылке GET чужая страница
/// разлогинивала бы пользователя без его ведома (docs/SPEC.md §15).
/// </summary>
public sealed class LogoutModel(ISender sender) : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Index", new { area = string.Empty });

    public async Task<IActionResult> OnPostAsync(string? returnUrl, CancellationToken cancellationToken)
    {
        await sender.Send(new SignOutCommand(), cancellationToken);

        return returnUrl is not null && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : Redirect(Url.Content("~/"));
    }
}