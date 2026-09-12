using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Carts;
using ZeldaArena.Application.Features.Carts.Commands.AddCartItem;
using ZeldaArena.Application.Features.Carts.Commands.ChangeCartItemQuantity;
using ZeldaArena.Application.Features.Carts.Commands.RemoveCartItem;
using ZeldaArena.Application.Features.Carts.Queries.GetCart;
using ZeldaArena.Domain.Common;
using ZeldaArena.Web.Constants;
using ZeldaArena.Web.Extensions;
using ZeldaArena.Web.Models.Carts;

namespace ZeldaArena.Web.Controllers;

[Route("cart")]
public sealed class CartController(ISender sender, IStringLocalizer<SharedResource> localizer) : Controller
{
    private const string StatusKey = TempDataKeys.StatusMessage;

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await sender.Send(new GetCartQuery(), cancellationToken));

    [HttpPost("items")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(AddToCartInputModel input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var back = Url.IsLocalUrl(input.ReturnUrl) ? input.ReturnUrl! : Url.Action(nameof(Index))!;

        if (!ModelState.IsValid)
        {
            TempData[StatusKey] = "!" + FirstModelError();
            return LocalRedirect(back);
        }

        var result = await sender.Send(new AddCartItemCommand(input.ProductId!.Value, input.Quantity), cancellationToken);

        TempData[StatusKey] = Message(result, "cart.added");

        return LocalRedirect(back);
    }

    [HttpPost("items/{productId:guid}/quantity")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeQuantity(
        Guid productId,
        CartQuantityInputModel input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (!ModelState.IsValid)
        {
            TempData[StatusKey] = "!" + FirstModelError();
            return RedirectToAction(nameof(Index));
        }

        var result = await sender.Send(new ChangeCartItemQuantityCommand(productId, input.Quantity), cancellationToken);

        TempData[StatusKey] = Message(result, "cart.quantity_changed");

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("items/{productId:guid}/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid productId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RemoveCartItemCommand(productId), cancellationToken);

        TempData[StatusKey] = Message(result, "cart.removed");

        return RedirectToAction(nameof(Index));
    }

    private string Message(Result<CartSummaryDto> result, string successKey) =>
        result.IsSuccess ? localizer[successKey].Value : "!" + localizer.ForError(result.Error);

    private string FirstModelError() =>
        ModelState.Values
            .SelectMany(entry => entry.Errors)
            .Select(error => error.ErrorMessage)
            .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text))
        ?? localizer["cart.invalid_input"].Value;
}