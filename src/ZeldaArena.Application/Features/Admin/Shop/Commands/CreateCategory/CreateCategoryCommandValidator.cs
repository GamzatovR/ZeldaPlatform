using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Shop.Commands.CreateCategory;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public const int MaxNameLength = 128;

    public CreateCategoryCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .WithMessage("Укажите название категории.")
            .MaximumLength(MaxNameLength)
            .WithMessage($"Название не длиннее {MaxNameLength} символов.");
    }
}