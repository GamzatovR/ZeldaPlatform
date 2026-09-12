using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Shop.Queries.GetProductsForAdmin;

public sealed class GetProductsForAdminQueryValidator : AbstractValidator<GetProductsForAdminQuery>
{
    public const int MaxSearchLength = 100;

    public GetProductsForAdminQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Номер страницы начинается с единицы.");

        RuleFor(query => query.Search)
            .MaximumLength(MaxSearchLength)
            .WithMessage($"Строка поиска не длиннее {MaxSearchLength} символов.");
    }
}