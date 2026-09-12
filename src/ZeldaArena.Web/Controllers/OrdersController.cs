using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Orders;
using ZeldaArena.Application.Features.Orders.Commands.CancelOrder;
using ZeldaArena.Application.Features.Orders.Queries.GetMyOrders;
using ZeldaArena.Application.Features.Orders.Queries.GetOrderDetails;
using ZeldaArena.Web.Constants;
using ZeldaArena.Web.Extensions;
using ZeldaArena.Web.Models.Orders;

namespace ZeldaArena.Web.Controllers;

[Authorize]
[Route("orders")]
public sealed class OrdersController(ISender sender, IStringLocalizer<SharedResource> localizer) : Controller
{
    private const string StatusKey = TempDataKeys.StatusMessage;

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetMyOrdersQuery filter, CancellationToken cancellationToken)
    {
        // Неразборчивый статус (?status=bogus) — испорченная ссылка, а не пустой фильтр.
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var result = await sender.Send(filter, cancellationToken);

        return View(new OrderListViewModel { Filter = filter, Result = result });
    }

    [HttpGet("{number}")]
    public async Task<IActionResult> Details(string number, CancellationToken cancellationToken)
    {
        if (!IsPlausibleNumber(number))
        {
            return NotFound();
        }

        var order = await sender.Send(new GetOrderDetailsQuery(number), cancellationToken);

        return order is null ? NotFound() : View(order);
    }

    [HttpPost("{number}/cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(string number, CancellationToken cancellationToken)
    {
        if (!IsPlausibleNumber(number))
        {
            return NotFound();
        }

        var result = await sender.Send(new CancelOrderCommand(number), cancellationToken);

        TempData[StatusKey] = result.IsSuccess
            ? localizer["order.canceled"].Value
            : "!" + localizer.ForError(result.Error);

        return RedirectToAction(nameof(Details), new { number });
    }

    private static bool IsPlausibleNumber(string number) =>
        !string.IsNullOrWhiteSpace(number) && number.Length <= OrderRules.MaxNumberLength;
}