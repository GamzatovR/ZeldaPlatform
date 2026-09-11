using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Account.Commands.EnableTwoFactor;
using ZeldaArena.Application.Features.Account.Queries.GetAuthenticatorSetup;
using ZeldaArena.Web.Extensions;
using ZeldaArena.Web.Models.Account;
using ZeldaArena.Web.Services;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account.Manage;

/// <summary>
/// Подключение аутентификатора (docs/SPEC.md §8.2). Второй фактор включается только
/// после того, как введённый код сошёлся: иначе неверно настроенное приложение
/// заперло бы человека снаружи собственной учётной записи.
/// </summary>
[Authorize]
public sealed class EnableAuthenticatorModel(
    ISender sender,
    IStringLocalizer<SharedResource> localizer)
    : PageModel
{
    [BindProperty]
    public EnableAuthenticatorViewModel Input { get; set; } = new();

    public string SharedKey { get; private set; } = string.Empty;

    /// <summary>
    /// QR-код ссылки otpauth:// картинкой в data-URI: строится на сервере и работает
    /// без JavaScript, а в разметку попадает как <c>&lt;img&gt;</c>, без <c>Html.Raw</c> (§15).
    /// </summary>
    public string QrCodeDataUri { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken) =>
        await LoadSetupAsync(cancellationToken) ? Page() : NotFound();

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await LoadSetupAsync(cancellationToken) ? Page() : NotFound();
        }

        var result = await sender.Send(
            new EnableTwoFactorCommand(Input.VerificationCode),
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddResultError(result, localizer);

            return await LoadSetupAsync(cancellationToken) ? Page() : NotFound();
        }

        TempData["RecoveryCodes"] = string.Join('\n', result.Value.Codes);
        TempData["StatusMessage"] = localizer["enable_authenticator.enabled"].Value;

        return RedirectToPage("./ShowRecoveryCodes");
    }

    private async Task<bool> LoadSetupAsync(CancellationToken cancellationToken)
    {
        var setup = await sender.Send(new GetAuthenticatorSetupQuery(), cancellationToken);

        if (setup is null)
        {
            return false;
        }

        SharedKey = setup.SharedKey;
        QrCodeDataUri = QrCodeRenderer.ToSvgDataUri(setup.AuthenticatorUri);

        return true;
    }
}