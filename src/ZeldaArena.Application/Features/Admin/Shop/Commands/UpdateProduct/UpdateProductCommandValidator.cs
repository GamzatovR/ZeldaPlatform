using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Shop.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Не указан товар.");

        Include(new ProductFieldsValidator());
    }
}