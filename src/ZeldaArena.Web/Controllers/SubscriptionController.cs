using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Subscriptions.Commands.CancelSubscription;
using ZeldaArena.Application.Features.Subscriptions.Queries.GetMySubscription;
using ZeldaArena.Application.Features.Subscriptions.Queries.GetSubscriptionPlans;
using ZeldaArena.Web.Constants;
using ZeldaArena.Web.Models.Billing;

namespace ZeldaArena.Web.Controllers;

[Authorize]
[Route("account/subscription")]
public sealed class SubscriptionController(
    ISender sender,
    IStringLocalizer<SharedResource> localizer)
    : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        string? required,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        var model = new SubscriptionPageViewModel
        {
            Plans = await sender.Send(new GetSubscriptionPlansQuery(), cancellationToken),
            Current = await sender.Send(new GetMySubscriptionQuery(), cancellationToken),
            RequiredFeature = required,

            // Открытый редирект: возвращаться можно только внутрь сайта.
            ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null,
        };

        return View(model);
    }

    [HttpPost("cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CancelSubscriptionCommand(), cancellationToken);

        TempData[TempDataKeys.StatusMessage] = result.IsSuccess
            ? localizer["subscription.cancelled"].Value
            : localizer[result.Error.Code].Value;

        return RedirectToAction(nameof(Index));
    }
}