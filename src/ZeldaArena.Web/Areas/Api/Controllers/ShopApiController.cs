using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

using ZeldaArena.Application.Features.Shop.Queries.GetProducts;
using ZeldaArena.Web.Controllers;

namespace ZeldaArena.Web.Areas.Api.Controllers;

/// <summary>Фильтр, сортировка и пагинация каталога.</summary>
[Route("api/shop")]
public sealed class ShopApiController(ISender sender, IStringLocalizer<SharedResource> localizer)
    : ApiControllerBase(localizer)
{
    [HttpGet("products")]
    public async Task<IActionResult> Products([FromQuery] GetProductsQuery filter, CancellationToken cancellationToken)
    {
        var result = await sender.Send(filter, cancellationToken);

        return ListPartial(
            "~/Views/Shop/_ProductList.cshtml",
            result,
            PageUrl(nameof(ShopController.Index), "Shop"));
    }
}