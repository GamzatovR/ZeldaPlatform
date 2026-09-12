using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Admin.Orders.Queries.GetOrdersForAdmin;
using ZeldaArena.Application.Features.Admin.Shop.Queries.GetProductsForAdmin;
using ZeldaArena.Web.Areas.Admin.Models.Orders;
using ZeldaArena.Web.Areas.Admin.Models.Shop;
using ZeldaArena.Web.Authorization;

namespace ZeldaArena.Web.Areas.Api.Controllers.Admin;

/// <summary>Таблицы товаров и заказов без перезагрузки (docs/SPEC.md §10.1, сценарий 12).</summary>
[Route("api/admin")]
[Authorize(Policy = PolicyNames.AdminOnly)]
public sealed class AdminShopApiController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : AdminApiControllerBase(localizer)
{
    public const string ProductTablePartial = "~/Areas/Admin/Views/Products/_ProductTable.cshtml";
    public const string OrderTablePartial = "~/Areas/Admin/Views/Orders/_OrderTable.cshtml";

    [HttpGet("products")]
    public async Task<IActionResult> Products([FromQuery] GetProductsForAdminQuery filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var result = await sender.Send(filter, cancellationToken);

        return ListPartial(
            ProductTablePartial,
            new ProductIndexViewModel { Filter = filter, Result = result },
            AdminPageUrl("Index", "Products"));
    }

    [HttpGet("orders")]
    public async Task<IActionResult> Orders([FromQuery] GetOrdersForAdminQuery filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var result = await sender.Send(filter, cancellationToken);

        return ListPartial(
            OrderTablePartial,
            new OrderIndexViewModel { Filter = filter, Result = result },
            AdminPageUrl("Index", "Orders"));
    }
}