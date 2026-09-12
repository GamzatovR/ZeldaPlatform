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

/// <summary>
/// Тарифы и текущая подписка (docs/SPEC.md §9.3, п. 17).
///
/// Сюда же уводит отказ фича-гейта: параметр <c>required</c> называет функцию,
/// которой не хватило, и страница объясняет, каким тарифом она открывается (§7.3).
///
/// Контроллер принимает только <see cref="ISender"/>, <see cref="IStringLocalizer{T}"/>
/// и <see cref="ILogger{T}"/> — правило 4 §5.2, проверяется архитектурным тестом.
/// Логики здесь нет: она в хендлерах.
/// </summary>
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

            // Открытый редирект: возвращаться можно только внутрь сайта (§15).
            ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null,
        };

        return View(model);
    }

    /// <summary>
    /// Отказ от автопродления. Доступ сохраняется до конца оплаченного срока
    /// (§7.5, п. 5), о чём страница и сообщает.
    /// </summary>
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