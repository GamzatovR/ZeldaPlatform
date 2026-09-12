using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Features.Account.Commands.SignInWithRecoveryCode;
using ZeldaArena.Web.Extensions;
using ZeldaArena.Web.Models.Account;
using ZeldaArena.Web.RateLimiting;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
[EnableRateLimiting(RateLimitPolicies.SignIn)]
public sealed class LoginWithRecoveryCodeModel(
    ISender sender,
    IStringLocalizer<SharedResource> localizer)
    : PageModel
{
    [BindProperty]
    public RecoveryCodeLoginViewModel Input { get; set; } = new();

    public string? ReturnUrl { get; private set; }

    public void OnGet(string? returnUrl) => ReturnUrl = LocalOrHome(returnUrl);

    public async Task<IActionResult> OnPostAsync(string? returnUrl, CancellationToken cancellationToken)
    {
        ReturnUrl = LocalOrHome(returnUrl);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await sender.Send(
            new SignInWithRecoveryCodeCommand(Input.RecoveryCode),
            cancellationToken);

        if (result.IsSuccess)
        {
            return LocalRedirect(ReturnUrl);
        }

        if (result.Error.Code == AccountErrors.TwoFactorSessionExpired.Code)
        {
            return RedirectToPage("./Login", new { returnUrl = ReturnUrl });
        }

        if (result.Error.Code == AccountErrors.LockedOut.Code)
        {
            return RedirectToPage("./Lockout");
        }

        ModelState.AddResultError(result, localizer);

        return Page();
    }

    private string LocalOrHome(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : Url.Content("~/");
}