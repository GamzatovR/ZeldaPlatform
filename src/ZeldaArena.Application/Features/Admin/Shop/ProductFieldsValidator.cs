using FluentValidation;

using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Application.Features.Admin.Shop;

public sealed class ProductFieldsValidator : AbstractValidator<IProductFields>
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 4000;
    public const int MaxSkuLength = 64;
    public const decimal MaxPrice = 1_000_000m;

    /// <summary>Склад учебного магазина: больше десяти тысяч единиц не бывает.</summary>
    public const int MaxStock = 10_000;

    public const string SkuPattern = "^[A-Za-z0-9-]{3,64}$";

    public ProductFieldsValidator()
    {
        RuleFor(fields => fields.Name)
            .NotEmpty()
            .WithMessage("Укажите название товара.")
            .MaximumLength(MaxNameLength)
            .WithMessage($"Название не длиннее {MaxNameLength} символов.");

        RuleFor(fields => fields.CategoryId)
            .NotEmpty()
            .WithMessage("Выберите категорию.");

        RuleFor(fields => fields.Price)
            .InclusiveBetween(0m, MaxPrice)
            .WithMessage($"Цена — от 0 до {MaxPrice}.");

        RuleFor(fields => fields.StockQuantity)
            .InclusiveBetween(0, MaxStock)
            .WithMessage($"Остаток — от 0 до {MaxStock}.");

        RuleFor(fields => fields.Description)
            .MaximumLength(MaxDescriptionLength)
            .WithMessage($"Описание не длиннее {MaxDescriptionLength} символов.");

        RuleFor(fields => fields.Image).ValidImageUpload();
    }
}