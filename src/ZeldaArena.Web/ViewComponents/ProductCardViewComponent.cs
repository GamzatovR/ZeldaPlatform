using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Shop.Queries.GetProducts;

namespace ZeldaArena.Web.ViewComponents;

/// <summary>
/// Карточка товара (docs/SPEC.md §10.2) — одна на каталог и витрину главной.
/// Данные приходят готовыми от вызывающей страницы: компонент ничего не запрашивает,
/// он только держит разметку в одном месте.
/// </summary>
public sealed class ProductCardViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(ProductListItemDto product) => View(product);
}