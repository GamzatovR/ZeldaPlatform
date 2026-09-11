using System.ComponentModel.DataAnnotations;

using ZeldaArena.Application.Features.Carts;

namespace ZeldaArena.Web.Models.Carts;

/// <summary>
/// Форма «в корзину» на карточке товара. Цены здесь нет: её берёт сервер (§15).
/// Диапазон количества повторяет валидатор команды — иначе поле, отвергнутое
/// FluentValidation, до Фазы 11 превращалось бы в ошибку сервера (урок Фазы 6).
/// </summary>
public sealed class AddToCartInputModel
{
    /// <summary>Nullable ради [Required]: у Guid нет «пустого» значения, и отсутствующее поле тихо стало бы Guid.Empty.</summary>
    [Required(ErrorMessage = "Не указан товар.")]
    public Guid? ProductId { get; set; }

    [Range(1, CartStockCheck.MaxQuantityPerLine, ErrorMessage = "Количество — от {1} до {2}.")]
    public int Quantity { get; set; } = 1;

    /// <summary>Куда вернуться после добавления: каталог с тем же фильтром или главная.</summary>
    public string? ReturnUrl { get; set; }
}