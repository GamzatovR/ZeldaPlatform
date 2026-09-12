using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Orders.Queries.GetOrderForAdmin;

public sealed class GetOrderForAdminQueryValidator : AbstractValidator<GetOrderForAdminQuery>
{
    public GetOrderForAdminQueryValidator()
    {
        RuleFor(query => query.Number).NotEmpty().WithMessage("Не указан номер заказа.");
    }
}