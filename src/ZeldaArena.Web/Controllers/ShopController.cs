using MediatR;

using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Shop.Queries.GetProducts;
using ZeldaArena.Application.Features.Shop.Queries.GetShopFilterOptions;
using ZeldaArena.Web.Models.Shop;

namespace ZeldaArena.Web.Controllers;

/// <summary>
/// Каталог магазина (docs/SPEC.md §9.3, п. 12). Тот же механизм фильтрации, что
/// у турниров, команд и игроков (§10.2): GET-форма, состояние в адресе, partial-список,
/// который отдаёт и <c>GET /api/shop/products</c>.
/// </summary>
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