using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Payments.Commands.ConfirmPayment;
using ZeldaArena.Application.Features.Payments.Commands.StartSubscriptionPayment;
using ZeldaArena.Application.Features.Payments.Queries.GetPaymentState;
using ZeldaArena.Web.Areas.Api.Models;
using ZeldaArena.Web.Authorization;
using ZeldaArena.Web.Controllers;
using ZeldaArena.Web.Models.Billing;
using ZeldaArena.Web.Payments;
using ZeldaArena.Web.RateLimiting;

namespace ZeldaArena.Web.Areas.Api.Controllers;

/// <summary>
/// Мнимая оплата без перезагрузки формы (docs/SPEC.md §7.6, §10.1, сценарии 7 и 8):
/// реквизиты → письмо с кодом → подтверждение. Хендлеры, лимиты и политика
/// подтверждённой почты те же, что у <see cref="PaymentController"/>.
///
/// Форма уходит как есть (<c>multipart/form-data</c>) и биндится в ту же ViewModel,
/// поэтому и DataAnnotations, и имена полей в ошибках совпадают с разметкой формы.
/// Номер карты, CVV и код дальше команды не идут: ответ их не содержит, в журнал
/// аудита они попадают замаскированными (§13).
/// </summary>
[Authorize(Policy = PolicyNames.EmailConfirmed)]
[Route("api/payments")]
public sealed class PaymentsApiController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : ApiControllerBase(localizer)
{
    private const string StatusKey = "StatusMessage";

    /// <summary>Создание платежа за подписку и отправка кода на почту.</summary>
    [HttpPost("")]
    [EnableRateLimiting(RateLimitPolicies.PaymentStart)]
    public async Task<IActionResult> Start([FromForm] CardPaymentViewModel model, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

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

        return result.IsSuccess
            ? Ok(new RedirectResponse(PageUrl(nameof(PaymentController.Confirm), "Payment", new { id = result.Value.PaymentId })))
            : Failure(result.Error);
    }

    /// <summary>
    /// Подтверждение кодом. Куда идти дальше, решает то же правило, что у формы;
    /// сообщение для следующей страницы кладётся в TempData, как при PRG.
    /// Неверный код с оставшимися попытками — 400 с их числом: страница обновит счётчик.
    /// </summary>
    [HttpPost("{id:guid}/confirm")]
    [EnableRateLimiting(RateLimitPolicies.PaymentConfirm)]
    public async Task<IActionResult> Confirm(
        Guid id,
        [FromForm] ConfirmPaymentViewModel model,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        var result = await sender.Send(new ConfirmPaymentCommand(id, model.ConfirmationCode), cancellationToken);

        // Перечитывается после попытки: она израсходована, и счётчик должен это показать.
        var state = await sender.Send(new GetPaymentStateQuery(id), cancellationToken);

        if (state is null)
        {
            return NotFound();
        }

        var outcome = PaymentConfirmationOutcome.Resolve(state, result, Localizer);

        if (outcome.StatusMessage is not null)
        {
            TempData[StatusKey] = outcome.StatusMessage;
        }

        switch (outcome.Step)
        {
            case PaymentNextStep.SubscriptionActivated:
                return Ok(new RedirectResponse(PageUrl(nameof(PaymentController.Success), "Payment")));
            case PaymentNextStep.OrderDetails:
                return Ok(new RedirectResponse(PageUrl(nameof(OrdersController.Details), "Orders", new { number = state.OrderNumber })));
            case PaymentNextStep.Cart:
                return Ok(new RedirectResponse(PageUrl(nameof(CartController.Index), "Cart")));
        }

        if (result.IsSuccess)
        {
            return Ok(new RedirectResponse(PageUrl(nameof(PaymentController.Confirm), "Payment", new { id })));
        }

        var failure = Failure(result.Error);
        ((ProblemDetails)failure.Value!).Extensions["attemptsLeft"] = state.AttemptsLeft;

        return failure;
    }
}
