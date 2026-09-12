using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Shop.Queries.GetProducts;
using ZeldaArena.Web.Constants;
using ZeldaArena.Web.Models.Shop;

namespace ZeldaArena.Web.ViewComponents;

/// <summary>Карточка товара — одна на каталог и витрину главной.</summary>
public sealed class ProductCardViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(ProductListItemDto product, string? returnFragment = null)
    {
        // Вернуться туда же, откуда добавляли.
        var path = ViewData[ListViewData.PagePath] as string ?? Request.Path.Value;
        var returnUrl = path + Request.QueryString
            + (string.IsNullOrEmpty(returnFragment) ? string.Empty : "#" + returnFragment);

        return View(new ProductCardViewModel(product, returnUrl));
    }
}