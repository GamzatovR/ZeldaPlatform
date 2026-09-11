using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Common.Models.Shop;
using ZeldaArena.Application.Features.Account.Queries.GetAccountProfile;
using ZeldaArena.Application.Features.Carts.Queries.GetCart;
using ZeldaArena.Application.Features.Orders.Commands.PlaceOrder;
using ZeldaArena.Web.Authorization;
using ZeldaArena.Web.Extensions;
using ZeldaArena.Web.Models.Billing;
using ZeldaArena.Web.Models.Checkout;
using ZeldaArena.Web.RateLimiting;

namespace ZeldaArena.Web.Controllers;

/// <summary>
/// Оформление заказа (docs/SPEC.md §9.3, п. 14): адрес и реквизиты одной формой,
/// дальше — общий для всех покупок ввод кода из письма (<see cref="PaymentController"/>).
///
/// Закрыто политикой подтверждённой почты: §8.2 требует подтверждённого адреса
/// перед оформлением заказа. Гость попадает сюда после входа, и его корзина к этому
/// моменту уже влита в пользовательскую (<c>CartCookieMiddleware</c>).
/// </summary>
[Authorize(Policy = PolicyNames.EmailConfirmed)]
[Route("checkout")]
public sealed class CheckoutController(ISender sender, IStringLocalizer<SharedResource> localizer) : Controller
{
    private const string StatusKey = "StatusMessage";

    private static readonly HashSet<string> CartErrors =
    [
        ShopErrors.CartEmpty.Code,
        ShopErrors.CartHasProblems.Code,
        ShopErrors.StockChanged.Code,
    ];

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var cart = await sender.Send(new GetCartQuery(), cancellationToken);

        if (!cart.CanCheckout)
        {
            return BackToCart(cart.IsEmpty ? ShopErrors.CartEmpty.Code : ShopErrors.CartHasProblems.Code);
        }

        var profile = await sender.Send(new GetAccountProfileQuery(), cancellationToken);

        return View(new CheckoutViewModel
        {
            Recipient = profile?.DisplayName ?? string.Empty,
            Card = new CardDetailsInputModel { ConfirmationEmail = profile?.Email ?? string.Empty },
            IdempotencyKey = Guid.CreateVersion7().ToString(),
            Cart = cart,
        });
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(RateLimitPolicies.PaymentStart)]
    public async Task<IActionResult> Index(CheckoutViewModel model, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!ModelState.IsValid)
        {
            return await RedisplayAsync(model, cancellationToken);
        }

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
            return RedirectToAction(nameof(PaymentController.Confirm), "Payment", new { id = result.Value.PaymentId });
        }

        // Беда с корзиной — не ошибка формы: исправлять её нужно на странице корзины,
        // где видно, какая позиция мешает.
        if (CartErrors.Contains(result.Error.Code))
        {
            return BackToCart(result.Error.Code);
        }

        ModelState.AddResultError(result, localizer);

        return await RedisplayAsync(model, cancellationToken);
    }

    /// <summary>Форма показывается снова с тем же составом корзины, что увидит сервер.</summary>
    private async Task<IActionResult> RedisplayAsync(CheckoutViewModel model, CancellationToken cancellationToken)
    {
        model.Cart = await sender.Send(new GetCartQuery(), cancellationToken);

        return model.Cart.IsEmpty ? BackToCart(ShopErrors.CartEmpty.Code) : View(nameof(Index), model);
    }

    private RedirectToActionResult BackToCart(string errorCode)
    {
        TempData[StatusKey] = "!" + localizer[errorCode].Value;

        return RedirectToAction(nameof(CartController.Index), "Cart");
    }
}