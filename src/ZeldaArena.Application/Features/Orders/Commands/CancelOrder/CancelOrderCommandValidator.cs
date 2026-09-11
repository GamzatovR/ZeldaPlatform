using FluentValidation;

namespace ZeldaArena.Application.Features.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(command => command.Number)
            .NotEmpty()
            .WithMessage("Не указан номер заказа.")
            .MaximumLength(OrderRules.MaxNumberLength)
            .WithMessage($"Номер заказа не длиннее {OrderRules.MaxNumberLength} символов.");
    }
}