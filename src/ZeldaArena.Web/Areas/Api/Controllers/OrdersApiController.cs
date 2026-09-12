using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Common.Models.Shop;
using ZeldaArena.Application.Features.Orders.Commands.PlaceOrder;
using ZeldaArena.Application.Features.Orders.Queries.GetMyOrders;
using ZeldaArena.Web.Areas.Api.Models;
using ZeldaArena.Web.Authorization;
using ZeldaArena.Web.Constants;
using ZeldaArena.Web.Controllers;
using ZeldaArena.Web.Models.Checkout;
using ZeldaArena.Web.RateLimiting;

namespace ZeldaArena.Web.Areas.Api.Controllers;

[Authorize]
[Route("api/orders")]
public sealed class OrdersApiController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : ApiControllerBase(localizer)
{
    private const string StatusKey = TempDataKeys.StatusMessage;

    private static readonly HashSet<string> CartErrors =
    [
        ShopErrors.CartEmpty.Code,
        ShopErrors.CartHasProblems.Code,
        ShopErrors.StockChanged.Code,
    ];

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetMyOrdersQuery filter, CancellationToken cancellationToken)
    {
        var result = await sender.Send(filter, cancellationToken);

        return ListPartial(
            "~/Views/Orders/_OrderList.cshtml",
            result,
            PageUrl(nameof(OrdersController.Index), "Orders"));
    }

    [HttpPost("")]
    [Authorize(Policy = PolicyNames.EmailConfirmed)]
    [EnableRateLimiting(RateLimitPolicies.PaymentStart)]
    public async Task<IActionResult> Place([FromForm] CheckoutViewModel model, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        var result = await sender.Send(
            new PlaceOrderCommand(
                model.Recipient,
                model.Phone,
                model.Country,
                model.City,
                model.Street,
                model.PostalCode,
                model.Card.CardNumber,
                model.Card.ExpiryMonth,
                model.Card.ExpiryYear,
                model.Card.Cvv,
                model.Card.ConfirmationEmail,
                model.IdempotencyKey),
            cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(new RedirectResponse(
                PageUrl(nameof(PaymentController.Confirm), "Payment", new { id = result.Value.PaymentId })));
        }

        if (CartErrors.Contains(result.Error.Code))
        {
            TempData[StatusKey] = "!" + Localizer[result.Error.Code].Value;

            return Ok(new RedirectResponse(PageUrl(nameof(CartController.Index), "Cart")));
        }

        return Failure(result.Error);
    }
}