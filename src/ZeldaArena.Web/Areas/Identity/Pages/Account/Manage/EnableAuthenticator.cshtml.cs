using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Account.Commands.EnableTwoFactor;
using ZeldaArena.Application.Features.Account.Queries.GetAuthenticatorSetup;
using ZeldaArena.Web.Constants;
using ZeldaArena.Web.Extensions;
using ZeldaArena.Web.Models.Account;
using ZeldaArena.Web.Services;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account.Manage;

[Authorize]
public sealed class EnableAuthenticatorModel(
    ISender sender,
    IStringLocalizer<SharedResource> localizer)
    : PageModel
{
    [BindProperty]
    public EnableAuthenticatorViewModel Input { get; set; } = new();

    public string SharedKey { get; private set; } = string.Empty;

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
        TempData[TempDataKeys.StatusMessage] = localizer["enable_authenticator.enabled"].Value;

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