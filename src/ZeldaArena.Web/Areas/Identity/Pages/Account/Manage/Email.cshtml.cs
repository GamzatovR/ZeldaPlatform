using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Account.Commands.RequestEmailChange;
using ZeldaArena.Application.Features.Account.Queries.GetAccountProfile;
using ZeldaArena.Web.Constants;
using ZeldaArena.Web.Extensions;
using ZeldaArena.Web.Models.Account;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account.Manage;

[Authorize]
public sealed class EmailModel(ISender sender, IStringLocalizer<SharedResource> localizer)
    : PageModel
{
    [BindProperty]
    public ChangeEmailViewModel Input { get; set; } = new();

    public string CurrentEmail { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var profile = await sender.Send(new GetAccountProfileQuery(), cancellationToken);

        if (profile is null)
        {
            return NotFound();
        }

        CurrentEmail = profile.Email;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await OnGetAsync(cancellationToken);
        }

        var result = await sender.Send(
            new RequestEmailChangeCommand(Input.NewEmail),
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddResultError(result, localizer);

            return await OnGetAsync(cancellationToken);
        }

        TempData[TempDataKeys.StatusMessage] = localizer["manage.email.sent"].Value;

        return RedirectToPage();
    }
}