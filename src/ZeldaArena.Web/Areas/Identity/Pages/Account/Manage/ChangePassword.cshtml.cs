using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Account.Commands.ChangePassword;
using ZeldaArena.Web.Constants;
using ZeldaArena.Web.Extensions;
using ZeldaArena.Web.Models.Account;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account.Manage;

[Authorize]
public sealed class ChangePasswordModel(ISender sender, IStringLocalizer<SharedResource> localizer)
    : PageModel
{
    [BindProperty]
    public ChangePasswordViewModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await sender.Send(
            new ChangePasswordCommand(Input.CurrentPassword, Input.NewPassword),
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddResultError(result, localizer);

            return Page();
        }

        TempData[TempDataKeys.StatusMessage] = localizer["manage.password.changed"].Value;

        return RedirectToPage();
    }
}