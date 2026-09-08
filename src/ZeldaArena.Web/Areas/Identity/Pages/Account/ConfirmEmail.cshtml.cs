using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Account.Commands.ConfirmEmail;

namespace ZeldaArena.Web.Areas.Identity.Pages.Account;

/// <summary>
/// Переход по ссылке из письма. Обрабатывается на GET: пользователь приходит сюда
/// кликом в почтовом клиенте, и формы для подтверждения быть не может.
/// </summary>
[AllowAnonymous]
public sealed class ConfirmEmailModel(ISender sender, IStringLocalizer<SharedResource> localizer)
    : PageModel
{
    public bool Succeeded { get; private set; }

    public string ErrorMessage { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(
        Guid userId,
        string? token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(token))
        {
            ErrorMessage = localizer["confirm_email.failure"];

            return Page();
        }

        var result = await sender.Send(new ConfirmEmailCommand(userId, token), cancellationToken);

        Succeeded = result.IsSuccess;

        if (result.IsFailure)
        {
            ErrorMessage = localizer[result.Error.Code];
        }

        return Page();
    }
}