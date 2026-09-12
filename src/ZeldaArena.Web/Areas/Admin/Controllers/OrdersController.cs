using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Admin.Orders.Commands.ChangeOrderStatus;
using ZeldaArena.Application.Features.Admin.Orders.Queries.GetOrderForAdmin;
using ZeldaArena.Application.Features.Admin.Orders.Queries.GetOrdersForAdmin;
using ZeldaArena.Web.Areas.Admin.Models.Orders;
using ZeldaArena.Web.Authorization;

namespace ZeldaArena.Web.Areas.Admin.Controllers;

[Route("admin/orders")]
[Authorize(Policy = PolicyNames.AdminOnly)]
public sealed class OrdersController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : AdminControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetOrdersForAdminQuery filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var result = await sender.Send(filter, cancellationToken);

        return View(new OrderIndexViewModel { Filter = filter, Result = result });
    }

    [HttpGet("{number}")]
    public async Task<IActionResult> Details(string number, CancellationToken cancellationToken)
    {
        var order = await sender.Send(new GetOrderForAdminQuery(number), cancellationToken);

        return order is null ? NotFound() : View(order);
    }

    [HttpPost("{number}/status")]
    public async Task<IActionResult> ChangeStatus(string number, OrderTransition transition, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var result = await sender.Send(new ChangeOrderStatusCommand(number, transition), cancellationToken);

        Report(
            result,
            localizer[transition switch
            {
                OrderTransition.Ship => "admin.order.shipped",
                OrderTransition.Complete => "admin.order.completed",
                _ => "admin.order.canceled",
            }].Value,
            localizer);

        return RedirectToAction(nameof(Details), new { number });
    }
}