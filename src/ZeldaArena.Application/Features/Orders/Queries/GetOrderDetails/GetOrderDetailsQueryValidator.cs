using FluentValidation;

namespace ZeldaArena.Application.Features.Orders.Queries.GetOrderDetails;

public sealed class GetOrderDetailsQueryValidator : AbstractValidator<GetOrderDetailsQuery>
{
    public GetOrderDetailsQueryValidator()
    {
        RuleFor(query => query.Number)
            .NotEmpty()
            .WithMessage("Не указан номер заказа.")
            .MaximumLength(OrderRules.MaxNumberLength)
            .WithMessage($"Номер заказа не длиннее {OrderRules.MaxNumberLength} символов.");
    }
}