using FluentValidation;

namespace ZeldaArena.Application.Features.Carts.Commands.ChangeCartItemQuantity;

public sealed class ChangeCartItemQuantityCommandValidator : AbstractValidator<ChangeCartItemQuantityCommand>
{
    public ChangeCartItemQuantityCommandValidator()
    {
        RuleFor(command => command.ProductId)
            .NotEmpty()
            .WithMessage("Не указан товар.");

        RuleFor(command => command.Quantity)
            .InclusiveBetween(1, CartStockCheck.MaxQuantityPerLine)
            .WithMessage($"Количество — от 1 до {CartStockCheck.MaxQuantityPerLine}.");
    }
}