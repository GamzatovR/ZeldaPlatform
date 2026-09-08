using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Account.Commands.ResetPassword;
using ZeldaArena.Web.Extensions;
using ZeldaArena.Web.Models.Account;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account;

/// <summary>
/// Установка нового пароля по ссылке из письма. Токен живёт 30 минут
/// (docs/SPEC.md §8.2); просроченная ссылка даёт понятный отказ, а не пустую форму.
/// </summary>
[AllowAnonymous]
public sealed class ResetPasswordModel(ISender sender, IStringLocalizer<SharedResource> localizer)
    : PageModel
{
    [BindProperty]
    public ResetPasswordViewModel Input { get; set; } = new();

    public IActionResult OnGet(Guid userId, string? token)
    {
        if (userId == Guid.Empty || string.IsNullOrEmpty(token))
        {
            return RedirectToPage("./Login");
        }

        Input.UserId = userId;
        Input.Token = token;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await sender.Send(
            new ResetPasswordCommand(Input.UserId, Input.Token, Input.NewPassword),
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddResultError(result, localizer);

            return Page();
        }

        return RedirectToPage("./ResetPasswordConfirmation");
    }
}