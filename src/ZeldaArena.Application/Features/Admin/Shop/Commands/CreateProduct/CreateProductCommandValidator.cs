using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Shop.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        Include(new ProductFieldsValidator());

        RuleFor(command => command.Sku)
            .NotEmpty()
            .WithMessage("Укажите артикул.")
            .Matches(ProductFieldsValidator.SkuPattern)
            .WithMessage("Артикул — от 3 до 64 латинских букв, цифр и дефисов.");
    }
}