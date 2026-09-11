using FluentValidation;

namespace ZeldaArena.Application.Features.Carts.Commands.AddCartItem;

public sealed class AddCartItemCommandValidator : AbstractValidator<AddCartItemCommand>
{
    public AddCartItemCommandValidator()
    {
        RuleFor(command => command.ProductId)
            .NotEmpty()
            .WithMessage("Не указан товар.");

        RuleFor(command => command.Quantity)
            .InclusiveBetween(1, CartStockCheck.MaxQuantityPerLine)
            .WithMessage($"Количество — от 1 до {CartStockCheck.MaxQuantityPerLine}.");
    }
}