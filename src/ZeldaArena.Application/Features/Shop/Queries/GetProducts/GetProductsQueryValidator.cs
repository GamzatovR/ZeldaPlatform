using FluentValidation;

using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Shop.Queries.GetProducts;

/// <summary>
/// Как у турниров: настоящие ошибки отвергаются, размер страницы и сортировка
/// нормализуются молча (§10.2). Перевёрнутый диапазон цен — не ошибка, а пустая
/// выборка: пустое состояние с подсказкой отвечает на него честнее отказа
/// (урок Фазы 6 с диапазоном дат).
/// </summary>
public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Номер страницы начинается с единицы.");

        RuleFor(query => query.Category)
            .MaximumLength(Slug.MaxLength)
            .WithMessage($"Слаг категории не длиннее {Slug.MaxLength} символов.");

        RuleFor(query => query.PriceMin)
            .GreaterThanOrEqualTo(0m)
            .When(query => query.PriceMin.HasValue)
            .WithMessage("Цена не может быть отрицательной.");

        RuleFor(query => query.PriceMax)
            .GreaterThanOrEqualTo(0m)
            .When(query => query.PriceMax.HasValue)
            .WithMessage("Цена не может быть отрицательной.");
    }
}