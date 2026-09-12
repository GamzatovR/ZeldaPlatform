using System.Globalization;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Account.Commands.RegisterUser;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Web.Extensions;
using ZeldaArena.Web.Models.Account;
using ZeldaArena.Web.RateLimiting;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
[EnableRateLimiting(RateLimitPolicies.Register)]
public sealed class RegisterModel(ISender sender, IStringLocalizer<SharedResource> localizer)
    : PageModel
{
    [BindProperty]
    public RegisterViewModel Input { get; set; } = new();

    public string? ReturnUrl { get; private set; }

    public void OnGet(string? returnUrl = null) => ReturnUrl = LocalOrHome(returnUrl);

    public async Task<IActionResult> OnPostAsync(string? returnUrl, CancellationToken cancellationToken)
    {
        ReturnUrl = LocalOrHome(returnUrl);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await sender.Send(
            new RegisterUserCommand(
                Input.Email,
                Input.Password,
                Input.DisplayName,
                CurrentCultureOrDefault()),
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddResultError(result, localizer);

            return Page();
        }

        return RedirectToPage("./RegisterConfirmation", new { email = Input.Email });
    }

    private static string? CurrentCultureOrDefault()
    {
        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        return SupportedCultures.IsSupported(culture) ? culture : null;
    }

    private string LocalOrHome(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : Url.Content("~/");
}