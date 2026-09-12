using System.ComponentModel.DataAnnotations;

using ZeldaArena.Application.Features.Admin.Shop;
using ZeldaArena.Application.Features.Admin.Shop.Queries.GetProductForEdit;
using ZeldaArena.Web.Validation;

namespace ZeldaArena.Web.Areas.Admin.Models.Shop;

/// <summary>Форма товара. Артикул задаётся только при создании: по нему товар ищут в учёте.</summary>
public sealed class ProductFormViewModel
{
    [Required(ErrorMessage = "Укажите артикул.")]
    [RegularExpression(ProductFieldsValidator.SkuPattern, ErrorMessage = "Артикул — от 3 до 64 латинских букв, цифр и дефисов.")]
    [Display(Name = "Артикул")]
    public string Sku { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите название товара.")]
    [StringLength(ProductFieldsValidator.MaxNameLength, ErrorMessage = "Название не длиннее {1} символов.")]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите категорию.")]
    [Display(Name = "Категория")]
    public Guid CategoryId { get; set; }

    [Range(typeof(decimal), "0", "1000000", ErrorMessage = "Цена — от {1} до {2}.")]
    [Display(Name = "Цена, ₽")]
    public decimal Price { get; set; }

    [Range(0, ProductFieldsValidator.MaxStock, ErrorMessage = "Остаток — от {1} до {2}.")]
    [Display(Name = "Остаток на складе")]
    public int StockQuantity { get; set; }

    [StringLength(ProductFieldsValidator.MaxDescriptionLength, ErrorMessage = "Описание не длиннее {1} символов.")]
    [Display(Name = "Описание")]
    public string? Description { get; set; }

    [Display(Name = "В продаже")]
    public bool IsActive { get; set; } = true;

    [ImageFile]
    [Display(Name = "Фотография")]
    public IFormFile? Image { get; set; }

    [Display(Name = "Убрать фотографию")]
    public bool RemoveImage { get; set; }

    public string? CurrentImagePath { get; set; }

    public static ProductFormViewModel From(ProductEditDto product)
    {
        ArgumentNullException.ThrowIfNull(product);

        return new ProductFormViewModel
        {
            Sku = product.Sku,
            Name = product.Name,
            CategoryId = product.CategoryId,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Description = product.Description,
            IsActive = product.IsActive,
            CurrentImagePath = product.ImagePath,
        };
    }
}