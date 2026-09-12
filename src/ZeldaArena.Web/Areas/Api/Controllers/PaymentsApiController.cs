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
using ZeldaArena.Web.Constants;
using ZeldaArena.Web.Controllers;
using ZeldaArena.Web.Models.Billing;
using ZeldaArena.Web.Payments;
using ZeldaArena.Web.RateLimiting;

namespace ZeldaArena.Web.Areas.Api.Controllers;

[Authorize(Policy = PolicyNames.EmailConfirmed)]
[Route("api/payments")]
public sealed class PaymentsApiController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : ApiControllerBase(localizer)
{
    private const string StatusKey = TempDataKeys.StatusMessage;

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