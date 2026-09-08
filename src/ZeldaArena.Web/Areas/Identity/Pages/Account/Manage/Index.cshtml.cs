using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Account.Commands.UpdateProfile;
using ZeldaArena.Application.Features.Account.Queries.GetAccountProfile;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Web.Extensions;
using ZeldaArena.Web.Models.Account;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account.Manage;

/// <summary>
/// Профиль пользователя. Чей профиль показывать, решает запрос по текущему
/// пользователю — идентификатора в адресе нет, поэтому чужой профиль не открыть
/// (docs/SPEC.md §15, IDOR).
/// </summary>
[Authorize]
public sealed class IndexModel(ISender sender, IStringLocalizer<SharedResource> localizer)
    : PageModel
{
    [BindProperty]
    public ProfileViewModel Input { get; set; } = new();

    public string Email { get; private set; } = string.Empty;

    public string Roles { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? LastLoginAt { get; private set; }

    public IEnumerable<SelectListItem> Cultures { get; } =
        SupportedCultures.All.Select(culture => new SelectListItem(culture, culture));

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var profile = await sender.Send(new GetAccountProfileQuery(), cancellationToken);

        if (profile is null)
        {
            return NotFound();
        }

        Email = profile.Email;
        Roles = string.Join(", ", profile.Roles);
        CreatedAt = profile.CreatedAt;
        LastLoginAt = profile.LastLoginAt;

        Input = new ProfileViewModel
        {
            DisplayName = profile.DisplayName,
            CountryCode = profile.CountryCode,
            PreferredCulture = profile.PreferredCulture ?? SupportedCultures.Default,
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await OnGetKeepingInputAsync(cancellationToken);
        }

        var result = await sender.Send(
            new UpdateProfileCommand(Input.DisplayName, Input.CountryCode, Input.PreferredCulture),
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddResultError(result, localizer);

            return await OnGetKeepingInputAsync(cancellationToken);
        }

        TempData["StatusMessage"] = localizer["manage.profile.saved"].Value;

        return RedirectToPage();
    }

    /// <summary>
    /// При неудачной отправке страница показывается заново, но введённое пользователем
    /// не затирается: перезагружаются только справочные поля — адрес, роли, даты.
    /// </summary>
    private async Task<IActionResult> OnGetKeepingInputAsync(CancellationToken cancellationToken)
    {
        var input = Input;

        var page = await OnGetAsync(cancellationToken);

        Input = input;

        return page;
    }
}