using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Account.Commands.ForgotPassword;
using ZeldaArena.Web.Extensions;
using ZeldaArena.Web.Models.Account;
using ZeldaArena.Web.RateLimiting;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
[EnableRateLimiting(RateLimitPolicies.PasswordRecovery)]
public sealed class ForgotPasswordModel(ISender sender, IStringLocalizer<SharedResource> localizer)
    : PageModel
{
    [BindProperty]
    public ForgotPasswordViewModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await sender.Send(new ForgotPasswordCommand(Input.Email), cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddResultError(result, localizer);

            return Page();
        }

        return RedirectToPage("./ForgotPasswordConfirmation");
    }
}