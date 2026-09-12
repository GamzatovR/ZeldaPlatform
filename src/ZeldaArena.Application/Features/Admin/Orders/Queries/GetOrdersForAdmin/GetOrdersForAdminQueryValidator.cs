using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Orders.Queries.GetOrdersForAdmin;

public sealed class GetOrdersForAdminQueryValidator : AbstractValidator<GetOrdersForAdminQuery>
{
    public const int MaxSearchLength = 100;

    public GetOrdersForAdminQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Номер страницы начинается с единицы.");

        RuleFor(query => query.Search)
            .MaximumLength(MaxSearchLength)
            .WithMessage($"Строка поиска не длиннее {MaxSearchLength} символов.");

        RuleFor(query => query.Status)
            .IsInEnum()
            .When(query => query.Status.HasValue)
            .WithMessage("Неизвестный статус заказа.");
    }
}