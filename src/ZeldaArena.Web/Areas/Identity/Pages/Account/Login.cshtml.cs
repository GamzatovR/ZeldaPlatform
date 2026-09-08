using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Features.Account.Commands.SignIn;
using ZeldaArena.Web.Extensions;
using ZeldaArena.Web.Models.Account;
using ZeldaArena.Web.RateLimiting;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account;

/// <summary>
/// Вход по паролю (docs/SPEC.md §8.2).
///
/// Страница ничего не решает сама: отправляет команду, разбирает <c>Result</c>
/// и выбирает, куда перейти. Ни <c>SignInManager</c>, ни <c>ApplicationUser</c> здесь
/// нет и быть не может — это типы Infrastructure, а правило 3 §5.2 держит их
/// за пределами Web.
/// </summary>
[AllowAnonymous]
[EnableRateLimiting(RateLimitPolicies.SignIn)]
public sealed class LoginModel(ISender sender, IStringLocalizer<SharedResource> localizer)
    : PageModel
{
    [BindProperty]
    public LoginViewModel Input { get; set; } = new();

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
            new SignInCommand(Input.Email, Input.Password, Input.RememberMe),
            cancellationToken);

        if (result.IsFailure)
        {
            // Блокировка после серии неудачных попыток заслуживает отдельной страницы:
            // на форме это выглядело бы как очередной неверный пароль.
            if (result.Error.Code == AccountErrors.LockedOut.Code)
            {
                return RedirectToPage("./Lockout");
            }

            ModelState.AddResultError(result, localizer);

            return Page();
        }

        return result.Value == SignInOutcome.RequiresTwoFactor
            ? RedirectToPage("./LoginWith2fa", new { returnUrl = ReturnUrl, Input.RememberMe })
            : LocalRedirect(ReturnUrl);
    }

    /// <summary>
    /// Открытый редирект закрывается здесь: адрес, пришедший из query-string, годится,
    /// только если он локальный (docs/SPEC.md §15).
    /// </summary>
    private string LocalOrHome(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : Url.Content("~/");
}