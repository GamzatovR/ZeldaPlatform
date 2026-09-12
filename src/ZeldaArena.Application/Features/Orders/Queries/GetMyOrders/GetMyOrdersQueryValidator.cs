using FluentValidation;

namespace ZeldaArena.Application.Features.Orders.Queries.GetMyOrders;

/// <summary>Как у остальных списков: размер страницы и сортировка нормализуются молча.</summary>
public sealed class GetMyOrdersQueryValidator : AbstractValidator<GetMyOrdersQuery>
{
    public GetMyOrdersQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Номер страницы начинается с единицы.");

        RuleFor(query => query.Status)
            .IsInEnum()
            .When(query => query.Status.HasValue)
            .WithMessage("Неизвестный статус заказа.");
    }
}