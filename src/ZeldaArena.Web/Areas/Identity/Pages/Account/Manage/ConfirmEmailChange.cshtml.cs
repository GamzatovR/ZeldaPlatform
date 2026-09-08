using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Account.Commands.ConfirmEmailChange;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account.Manage;

/// <summary>
/// Переход по ссылке из письма на новый адрес. Страница открыта анонимно:
/// ссылку читают в почтовом клиенте, который вполне может быть в другом браузере,
/// без сессии. Безопасность держится на подписанном токене, а не на cookie.
/// </summary>
[AllowAnonymous]
public sealed class ConfirmEmailChangeModel(
    ISender sender,
    IStringLocalizer<SharedResource> localizer)
    : PageModel
{
    public bool Succeeded { get; private set; }

    public string ErrorMessage { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(
        Guid userId,
        string? email,
        string? token,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            ErrorMessage = localizer["manage.email.change_failed"];

            return Page();
        }

        var result = await sender.Send(
            new ConfirmEmailChangeCommand(userId, email, token),
            cancellationToken);

        Succeeded = result.IsSuccess;

        if (result.IsFailure)
        {
            ErrorMessage = localizer[result.Error.Code];
        }

        return Page();
    }
}