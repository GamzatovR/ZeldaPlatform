using FluentValidation;

using ZeldaArena.Application.Features.Admin.Shop.Commands.CreateCategory;

namespace ZeldaArena.Application.Features.Admin.Shop.Commands.RenameCategory;

public sealed class RenameCategoryCommandValidator : AbstractValidator<RenameCategoryCommand>
{
    public RenameCategoryCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Не указана категория.");

        RuleFor(command => command.Name)
            .NotEmpty()
            .WithMessage("Укажите название категории.")
            .MaximumLength(CreateCategoryCommandValidator.MaxNameLength)
            .WithMessage($"Название не длиннее {CreateCategoryCommandValidator.MaxNameLength} символов.");
    }
}