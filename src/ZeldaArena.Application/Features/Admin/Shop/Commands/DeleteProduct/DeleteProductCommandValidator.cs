using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Shop.Commands.DeleteProduct;

public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Не указан товар.");
    }
}