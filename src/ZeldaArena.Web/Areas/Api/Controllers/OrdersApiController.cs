using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Orders.Queries.GetMyOrders;
using ZeldaArena.Web.Controllers;

namespace ZeldaArena.Web.Areas.Api.Controllers;

/// <summary>
/// История заказов: фильтр по статусу и пагинация (docs/SPEC.md §10.1, сценарий 3).
/// Владельца определяет сценарий — чужих заказов в ответе нет по построению (§15, IDOR).
/// </summary>
[Authorize]
[Route("api/orders")]
public sealed class OrdersApiController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : ApiControllerBase(localizer)
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetMyOrdersQuery filter, CancellationToken cancellationToken)
    {
        var result = await sender.Send(filter, cancellationToken);

        return ListPartial(
            "~/Views/Orders/_OrderList.cshtml",
            result,
            Url.Action(nameof(OrdersController.Index), "Orders", new { area = string.Empty }));
    }
}
