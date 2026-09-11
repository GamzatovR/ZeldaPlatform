using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Features.Payments.Commands.CancelPayment;
using ZeldaArena.Application.Features.Payments.Commands.ConfirmPayment;
using ZeldaArena.Application.Features.Payments.Commands.ResendPaymentCode;
using ZeldaArena.Application.Features.Payments.Commands.StartSubscriptionPayment;
using ZeldaArena.Application.Features.Payments.Queries.GetPaymentState;
using ZeldaArena.Application.Features.Subscriptions.Queries.GetSubscriptionPlans;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Web.Authorization;
using ZeldaArena.Web.Constants;
using ZeldaArena.Web.Extensions;
using ZeldaArena.Web.Models.Billing;
using ZeldaArena.Web.Payments;
using ZeldaArena.Web.RateLimiting;

namespace ZeldaArena.Web.Controllers;

/// <summary>
/// Мнимая оплата (docs/SPEC.md §7.6): реквизиты → письмо с кодом → подтверждение.
/// Реквизиты подписки вводятся здесь, реквизиты заказа — на странице оформления
/// (<see cref="CheckoutController"/>); ввод кода, повторная отправка и отмена у них общие.
///
/// Это обычные формы. С JavaScript их отправку перехватывает ajax-form.js и шлёт
/// в <c>POST /api/payments</c> и <c>/api/payments/{id}/confirm</c> (§10.1) — те же
/// хендлеры и то же правило исхода (<see cref="PaymentConfirmationOutcome"/>). Без
/// JavaScript формы работают как раньше — прогрессивное улучшение §9.1.
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
    private const string StatusKey = TempDataKeys.StatusMessage;

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
                model.Card.CardNumber,
                model.Card.ExpiryMonth,
                model.Card.ExpiryYear,
                model.Card.Cvv,
                model.Card.ConfirmationEmail,
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

        // Завершённый платёж за заказ вводить нечем: исход виден на странице заказа.
        if (IsFinishedOrderPayment(state))
        {
            return RedirectToAction(nameof(OrdersController.Details), "Orders", new { number = state.OrderNumber });
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

        Result? result = null;

        if (ModelState.IsValid)
        {
            result = await sender.Send(new ConfirmPaymentCommand(id, model.ConfirmationCode), cancellationToken);
        }

        // Состояние перечитывается: попытка израсходована, и на форме должно быть
        // видно, сколько их осталось.
        var state = await sender.Send(new GetPaymentStateQuery(id), cancellationToken);

        if (state is null)
        {
            return NotFound();
        }

        var outcome = PaymentConfirmationOutcome.Resolve(state, result, localizer);

        if (outcome.StatusMessage is not null)
        {
            TempData[StatusKey] = outcome.StatusMessage;
        }

        switch (outcome.Step)
        {
            case PaymentNextStep.SubscriptionActivated:
                return RedirectToAction(nameof(Success));
            case PaymentNextStep.OrderDetails:
                return RedirectToAction(nameof(OrdersController.Details), "Orders", new { number = state.OrderNumber });
            case PaymentNextStep.Cart:
                return RedirectToAction(nameof(CartController.Index), "Cart");
        }

        if (result is { IsFailure: true })
        {
            ModelState.AddResultError(result, localizer);
        }

        return View(ToViewModel(state));
    }

    [HttpPost("{id:guid}/resend")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(RateLimitPolicies.PaymentConfirm)]
    public async Task<IActionResult> Resend(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ResendPaymentCodeCommand(id), cancellationToken);

        TempData[StatusKey] = result.IsSuccess
            ? localizer["payment.code_sent"].Value
            : localizer.ForError(result.Error);

        return RedirectToAction(nameof(Confirm), new { id });
    }

    /// <summary>
    /// Отмена платежа за подписку возвращает к тарифам, за заказ — в корзину: заказ
    /// отменён, товары вернулись в неё (docs/adr/ADR-0009).
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var state = await sender.Send(new GetPaymentStateQuery(id), cancellationToken);
        var result = await sender.Send(new CancelPaymentCommand(id), cancellationToken);

        if (state?.Purpose == PaymentPurpose.Order)
        {
            if (result.IsSuccess)
            {
                TempData[StatusKey] = localizer["order.payment_canceled"].Value;
                return RedirectToAction(nameof(CartController.Index), "Cart");
            }

            // Платёж уже завершён (оплачен или отменён раньше) — товары в корзину
            // не возвращались, и отправлять туда незачем: исход виден на странице заказа.
            TempData[StatusKey] = "!" + localizer.ForError(result.Error);
            return RedirectToAction(nameof(OrdersController.Details), "Orders", new { number = state.OrderNumber });
        }

        return RedirectToAction(nameof(SubscriptionController.Index), "Subscription");
    }

    [HttpGet("success")]
    public IActionResult Success() => View();

    private static bool IsFinishedOrderPayment(PaymentStateDto state) =>
        state.Purpose == PaymentPurpose.Order && state.OrderNumber is not null && state.Status != PaymentStatus.Pending;

    private static ConfirmPaymentViewModel ToViewModel(
        PaymentStateDto state) =>
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