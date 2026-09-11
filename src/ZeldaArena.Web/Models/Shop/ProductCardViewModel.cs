using ZeldaArena.Application.Features.Shop.Queries.GetProducts;

namespace ZeldaArena.Web.Models.Shop;

/// <summary>Карточка товара и адрес, куда форма «в корзину» вернёт покупателя.</summary>
public sealed record ProductCardViewModel(ProductListItemDto Product, string ReturnUrl);