using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Features.Account.Queries.IsEmailAvailable;
using ZeldaArena.Web.RateLimiting;

namespace ZeldaArena.Web.Areas.Api.Controllers;

[Route("api/account")]
public sealed class AccountApiController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : ApiControllerBase(localizer)
{
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