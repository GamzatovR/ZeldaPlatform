using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Account.Commands.ResendEmailConfirmation;
using ZeldaArena.Web.Constants;
using ZeldaArena.Web.Extensions;
using ZeldaArena.Web.Models.Account;
using ZeldaArena.Web.RateLimiting;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account;

/// <summary>
/// Повторная отправка письма с подтверждением. Ответ одинаков для любого адреса,
/// поэтому по нему нельзя узнать, заведён ли он на портале (docs/SPEC.md §8.2).
/// </summary>
[AllowAnonymous]
[EnableRateLimiting(RateLimitPolicies.EmailDelivery)]
public sealed class ResendEmailConfirmationModel(
    ISender sender,
    IStringLocalizer<SharedResource> localizer)
    : PageModel
{
    [BindProperty]
    public ResendEmailConfirmationViewModel Input { get; set; } = new();

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
            new ResendEmailConfirmationCommand(Input.Email),
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddResultError(result, localizer);

            return Page();
        }

        TempData[TempDataKeys.StatusMessage] = localizer["resend_confirmation.sent"].Value;

        return RedirectToPage();
    }
}