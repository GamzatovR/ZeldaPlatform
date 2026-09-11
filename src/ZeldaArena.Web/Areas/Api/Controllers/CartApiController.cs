using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Carts;
using ZeldaArena.Application.Features.Carts.Commands.AddCartItem;
using ZeldaArena.Application.Features.Carts.Commands.ChangeCartItemQuantity;
using ZeldaArena.Application.Features.Carts.Commands.RemoveCartItem;
using ZeldaArena.Application.Features.Carts.Queries.GetCart;
using ZeldaArena.Domain.Common;
using ZeldaArena.Web.Areas.Api.Models;
using ZeldaArena.Web.Models.Carts;

namespace ZeldaArena.Web.Areas.Api.Controllers;

/// <summary>
/// Корзина без перезагрузки (docs/SPEC.md §10.1, сценарии 5 и 6). Команды те же,
/// что у формы <c>CartController</c>: владельца корзины определяет сценарий, в запросе
/// нет ни идентификатора корзины, ни цены (§15, IDOR и пересчёт на сервере).
///
/// Операции отвечают JSON со счётчиком мини-корзины; таблица с пересчитанным итогом —
/// отдельный <c>GET /api/cart</c>, тот же partial, что рисует страница.
/// </summary>
[Route("api/cart")]
public sealed class CartApiController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : ApiControllerBase(localizer)
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        PartialView("~/Views/Cart/_CartTable.cshtml", await sender.Send(new GetCartQuery(), cancellationToken));

    /// <summary>«В корзину» с карточки товара: форма карточки уходит как есть.</summary>
    [HttpPost("items")]
    public async Task<IActionResult> Add([FromForm] AddToCartInputModel input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var result = await sender.Send(new AddCartItemCommand(input.ProductId!.Value, input.Quantity), cancellationToken);

        return Respond(result, "cart.added");
    }

    [HttpPatch("items/{productId:guid}")]
    public async Task<IActionResult> ChangeQuantity(
        Guid productId,
        [FromBody] CartQuantityInputModel input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var result = await sender.Send(new ChangeCartItemQuantityCommand(productId, input.Quantity), cancellationToken);

        return Respond(result, "cart.quantity_changed");
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<IActionResult> Remove(Guid productId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RemoveCartItemCommand(productId), cancellationToken);

        return Respond(result, "cart.removed");
    }

    private IActionResult Respond(Result<CartSummaryDto> result, string successKey) =>
        result.IsSuccess
            ? Ok(new CartOperationResponse(
                result.Value.ItemCount,
                Localizer["cart.mini_label", result.Value.ItemCount].Value,
                Localizer[successKey].Value))
            : Failure(result.Error);
}
