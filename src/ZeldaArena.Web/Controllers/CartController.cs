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

/// <summary>
/// Корзина (docs/SPEC.md §9.3, п. 13). Работает для гостя и для пользователя: владельца
/// определяет сценарий, а не запрос, поэтому ни идентификатора корзины, ни цены
/// в формах нет.
///
/// Сейчас это обычные POST-формы с переходом обратно (PRG) — прогрессивное улучшение
/// §9.1: корзина обязана работать без JavaScript. AJAX-эндпоинты
/// <c>POST/PATCH/DELETE /api/cart/items</c> работают поверх тех же команд.
/// </summary>
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

    /// <summary>
    /// Исход действия — в TempData и обратно. Ошибка помечается ведущим «!»,
    /// как заведено в <c>_StatusMessage</c> с Фазы 3.
    /// </summary>
    private string Message(Result<CartSummaryDto> result, string successKey) =>
        result.IsSuccess ? localizer[successKey].Value : "!" + localizer.ForError(result.Error);

    /// <summary>
    /// Форма, не прошедшая проверку модели, — без JavaScript или в обход клиентской
    /// валидации. Сообщение уже переведено локализатором DataAnnotations.
    /// </summary>
    private string FirstModelError() =>
        ModelState.Values
            .SelectMany(entry => entry.Errors)
            .Select(error => error.ErrorMessage)
            .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text))
        ?? localizer["cart.invalid_input"].Value;
}