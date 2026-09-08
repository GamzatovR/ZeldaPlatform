using System.Globalization;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Account.Commands.RegisterUser;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Web.Extensions;
using ZeldaArena.Web.Models.Account;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account;

/// <summary>
/// Регистрация. Учётная запись создаётся сразу, но войти по ней нельзя, пока
/// не подтверждён адрес, поэтому после успеха идёт не вход, а страница
/// «проверьте почту» (docs/SPEC.md §8.2).
/// </summary>
[AllowAnonymous]
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

    /// <summary>
    /// Язык интерфейса берётся из текущей культуры. Неподдерживаемая культура
    /// заменяется на null, а не отправляется как есть: иначе валидатор отверг бы
    /// регистрацию за то, что у пользователя система, например, на немецком.
    /// Переключатель языка и UseRequestLocalization появятся в Фазе 11 (docs/SPEC.md §9.5).
    /// </summary>
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