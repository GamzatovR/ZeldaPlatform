using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Shop.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Не указана категория.");
    }
}