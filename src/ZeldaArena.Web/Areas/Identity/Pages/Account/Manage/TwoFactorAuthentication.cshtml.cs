using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Account.Commands.DisableTwoFactor;
using ZeldaArena.Application.Features.Account.Commands.GenerateRecoveryCodes;
using ZeldaArena.Application.Features.Account.Queries.GetTwoFactorStatus;
using ZeldaArena.Web.Constants;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account.Manage;

[Authorize]
public sealed class TwoFactorAuthenticationModel(
    ISender sender,
    IStringLocalizer<SharedResource> localizer)
    : PageModel
{
    public TwoFactorStatusDto? Status { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Status = await sender.Send(new GetTwoFactorStatusQuery(), cancellationToken);

    public async Task<IActionResult> OnPostDisableAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DisableTwoFactorCommand(), cancellationToken);

        TempData[TempDataKeys.StatusMessage] = result.IsSuccess
            ? localizer["manage.two_factor.disabled_notice"].Value

            // Восклицательный знак — признак ошибки для _StatusMessage.
            : "!" + localizer[result.Error.Code].Value;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostGenerateCodesAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GenerateRecoveryCodesCommand(), cancellationToken);

        if (result.IsFailure)
        {
            TempData[TempDataKeys.StatusMessage] = "!" + localizer[result.Error.Code].Value;

            return RedirectToPage();
        }

        // Коды показываются один раз, поэтому переносятся на страницу показа
        // через TempData и в адресную строку не попадают.
        TempData["RecoveryCodes"] = string.Join('\n', result.Value.Codes);
        TempData[TempDataKeys.StatusMessage] = localizer["recovery_codes.regenerated"].Value;

        return RedirectToPage("./ShowRecoveryCodes");
    }
}