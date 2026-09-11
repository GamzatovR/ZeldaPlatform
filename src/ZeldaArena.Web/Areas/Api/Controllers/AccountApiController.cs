using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Features.Account.Queries.IsEmailAvailable;
using ZeldaArena.Web.RateLimiting;

namespace ZeldaArena.Web.Areas.Api.Controllers;

/// <summary>
/// Remote-валидация занятости адреса на форме регистрации (docs/SPEC.md §10.1,
/// сценарий 11; §15). Формат ответа задан jquery-validation: <c>true</c> — поле
/// в порядке, строка — текст ошибки, который покажут под полем.
/// </summary>
[Route("api/account")]
public sealed class AccountApiController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : ApiControllerBase(localizer)
{
    /// <summary>
    /// Имя параметра — имя поля формы: на странице регистрации это <c>Input.Email</c>,
    /// потому что модель привязана свойством <c>Input</c>. Голый <c>email</c> принимается
    /// для вызова вне этой формы.
    ///
    /// Тип ответа указан явно: строку MVC отдал бы как text/plain, а jquery-validation
    /// разбирает ответ как JSON и на голом тексте молча не показывает ошибку.
    /// </summary>
    [HttpGet("check-email")]
    [Produces("application/json")]
    [EnableRateLimiting(RateLimitPolicies.EmailCheck)]
    public async Task<IActionResult> CheckEmail(
        [FromQuery(Name = "Input.Email")] string? formEmail,
        [FromQuery] string? email,
        CancellationToken cancellationToken)
    {
        var available = await sender.Send(new IsEmailAvailableQuery(formEmail ?? email ?? string.Empty), cancellationToken);

        return available
            ? Ok(true)
            : Ok(Localizer[AccountErrors.EmailAlreadyTaken.Code].Value);
    }
}
