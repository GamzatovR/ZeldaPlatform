using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Payments.Commands.CancelPayment;
using ZeldaArena.Application.Features.Payments.Commands.ConfirmPayment;
using ZeldaArena.Application.Features.Payments.Commands.ResendPaymentCode;
using ZeldaArena.Application.Features.Payments.Commands.StartSubscriptionPayment;
using ZeldaArena.Application.Features.Payments.Queries.GetPaymentState;
using ZeldaArena.Application.Features.Subscriptions.Queries.GetSubscriptionPlans;
using ZeldaArena.Web.Authorization;
using ZeldaArena.Web.Extensions;
using ZeldaArena.Web.Models.Billing;
using ZeldaArena.Web.RateLimiting;

namespace ZeldaArena.Web.Controllers;

/// <summary>
/// Мнимая оплата подписки (docs/SPEC.md §7.6): реквизиты → письмо с кодом →
/// подтверждение.
///
/// Пока это обычные формы. AJAX-эндпоинты <c>POST /api/payments</c> и
/// <c>/api/payments/{id}/confirm</c> из §10.1 появятся в Фазе 8 поверх этих же
/// хендлеров, страница перестанет перезагружаться, а логика не изменится.
/// Форма при этом продолжит работать без JavaScript — прогрессивное улучшение §9.1.
///
/// Покупка закрыта политикой подтверждённой почты: §8.2 требует подтверждённого
/// адреса перед оплатой.
/// </summary>
[Authorize(Policy = PolicyNames.EmailConfirmed)]
[Route("account/payment")]
public sealed class PaymentController(
    ISender sender,
    IStringLocalizer<SharedResource> localizer)
    : Controller
{
    /// <summary>Форма реквизитов. Ключ идемпотентности выдаётся здесь и живёт до отправки.</summary>
    [HttpGet("card")]
    public async Task<IActionResult> Card(Guid planId, CancellationToken cancellationToken)
    {
        var plans = await sender.Send(new GetSubscriptionPlansQuery(), cancellationToken);
        var plan = plans.FirstOrDefault(item => item.Id == planId);

        if (plan is null || plan.IsFree)
        {
            return RedirectToAction(nameof(SubscriptionController.Index), "Subscription");
        }

        return View(new CardPaymentViewModel
        {
            PlanId = plan.Id,
            PlanName = plan.Name,
            Price = plan.Price,
            Currency = plan.Currency,
            IdempotencyKey = Guid.CreateVersion7().ToString(),
        });
    }

    [HttpPost("card")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(RateLimitPolicies.PaymentStart)]
    public async Task<IActionResult> Card(CardPaymentViewModel model, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await sender.Send(
            new StartSubscriptionPaymentCommand(
                model.PlanId,
                model.CardNumber,
                model.ExpiryMonth,
                model.ExpiryYear,
                model.Cvv,
                model.ConfirmationEmail,
                model.IdempotencyKey),
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddResultError(result, localizer);

            return View(model);
        }

        return RedirectToAction(nameof(Confirm), new { id = result.Value.PaymentId });
    }

    /// <summary>Ввод кода: таймер, остаток попыток и кнопка повторной отправки.</summary>
    [HttpGet("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var state = await sender.Send(new GetPaymentStateQuery(id), cancellationToken);

        if (state is null)
        {
            return NotFound();
        }

        return View(ToViewModel(state));
    }

    [HttpPost("{id:guid}/confirm")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(RateLimitPolicies.PaymentConfirm)]
    public async Task<IActionResult> Confirm(
        Guid id,
        ConfirmPaymentViewModel model,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (ModelState.IsValid)
        {
            var result = await sender.Send(
                new ConfirmPaymentCommand(id, model.ConfirmationCode),
                cancellationToken);

            if (result.IsSuccess)
            {
                return RedirectToAction(nameof(Success));
            }

            ModelState.AddResultError(result, localizer);
        }

        // Состояние перечитывается: попытка израсходована, и на форме должно быть
        // видно, сколько их осталось.
        var state = await sender.Send(new GetPaymentStateQuery(id), cancellationToken);

        return state is null ? NotFound() : View(ToViewModel(state));
    }

    [HttpPost("{id:guid}/resend")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(RateLimitPolicies.PaymentConfirm)]
    public async Task<IActionResult> Resend(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ResendPaymentCodeCommand(id), cancellationToken);

        TempData["StatusMessage"] = result.IsSuccess
            ? localizer["payment.code_sent"].Value
            : localizer[result.Error.Code].Value;

        return RedirectToAction(nameof(Confirm), new { id });
    }

    [HttpPost("{id:guid}/cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new CancelPaymentCommand(id), cancellationToken);

        return RedirectToAction(nameof(SubscriptionController.Index), "Subscription");
    }

    [HttpGet("success")]
    public IActionResult Success() => View();

    private static ConfirmPaymentViewModel ToViewModel(
        Application.Common.Models.Billing.PaymentStateDto state) =>
        new()
        {
            PaymentId = state.PaymentId,
            MaskedEmail = state.MaskedEmail,
            AttemptsLeft = state.AttemptsLeft,
            ExpiresAt = state.ConfirmationExpiresAt,
            CanResendNow = state.CanResendNow,
            Amount = state.Amount,
            Currency = state.Currency,
        };
}