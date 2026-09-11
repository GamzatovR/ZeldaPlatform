using System.ComponentModel.DataAnnotations;

using ZeldaArena.Application.Features.Carts;

namespace ZeldaArena.Web.Models.Carts;

/// <summary>Поле количества в строке корзины.</summary>
public sealed class CartQuantityInputModel
{
    [Range(1, CartStockCheck.MaxQuantityPerLine, ErrorMessage = "Количество — от {1} до {2}.")]
    public int Quantity { get; set; }
}