using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Orders.Commands.ChangeOrderStatus;

public sealed class ChangeOrderStatusCommandValidator : AbstractValidator<ChangeOrderStatusCommand>
{
    public ChangeOrderStatusCommandValidator()
    {
        RuleFor(command => command.Number).NotEmpty().WithMessage("Не указан номер заказа.");
        RuleFor(command => command.Transition).IsInEnum().WithMessage("Неизвестное действие с заказом.");
    }
}