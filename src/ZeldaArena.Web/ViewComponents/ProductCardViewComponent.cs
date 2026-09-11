using Microsoft.AspNetCore.Mvc;

using ZeldaArena.Application.Features.Shop.Queries.GetProducts;
using ZeldaArena.Web.Models.Shop;

namespace ZeldaArena.Web.ViewComponents;

/// <summary>
/// Карточка товара (docs/SPEC.md §10.2) — одна на каталог и витрину главной.
/// Данные приходят готовыми от вызывающей страницы: компонент ничего не запрашивает,
/// он держит разметку в одном месте и знает, куда вернуть покупателя после «в корзину».
/// </summary>
public sealed class ProductCardViewComponent : ViewComponent
{
    /// <param name="product">Карточка из каталога или витрины.</param>
    /// <param name="returnFragment">
    /// Якорь секции, куда вернуться: на главной витрина далеко внизу, и без якоря
    /// сообщение «товар добавлен» осталось бы за пределами экрана.
    /// </param>
    public IViewComponentResult Invoke(ProductListItemDto product, string? returnFragment = null)
    {
        // Вернуться туда же, откуда добавляли: каталог с тем же фильтром и страницей.
        var returnUrl = Request.Path + Request.QueryString
            + (string.IsNullOrEmpty(returnFragment) ? string.Empty : "#" + returnFragment);

        return View(new ProductCardViewModel(product, returnUrl));
    }
}