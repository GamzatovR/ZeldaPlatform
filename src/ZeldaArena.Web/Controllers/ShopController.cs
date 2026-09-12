using MediatR;

using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Shop.Queries.GetProducts;
using ZeldaArena.Application.Features.Shop.Queries.GetShopFilterOptions;
using ZeldaArena.Web.Models.Shop;

namespace ZeldaArena.Web.Controllers;

[Route("shop")]
public sealed class ShopController(ISender sender) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GetProductsQuery filter, CancellationToken cancellationToken)
    {
        // Неразборчивое значение (priceMin=abc, inStock=maybe) — испорченная ссылка,
        // а не пустой фильтр: молча выбросить условие значило бы показать не то.
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var result = await sender.Send(filter, cancellationToken);
        var categories = await sender.Send(new GetShopFilterOptionsQuery(), cancellationToken);

        return View(new ShopListViewModel { Filter = filter, Categories = categories, Result = result });
    }
}