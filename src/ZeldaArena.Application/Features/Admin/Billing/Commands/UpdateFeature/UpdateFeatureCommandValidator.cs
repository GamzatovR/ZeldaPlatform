using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.UpdateFeature;

public sealed class UpdateFeatureCommandValidator : AbstractValidator<UpdateFeatureCommand>
{
    public const int MaxNameLength = 128;
    public const int MaxDescriptionLength = 512;

    public UpdateFeatureCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Не указана функция.");

        RuleFor(command => command.Name)
            .NotEmpty()
            .WithMessage("Укажите название функции.")
            .MaximumLength(MaxNameLength)
            .WithMessage($"Название не длиннее {MaxNameLength} символов.");

        RuleFor(command => command.Description)
            .MaximumLength(MaxDescriptionLength)
            .WithMessage($"Описание не длиннее {MaxDescriptionLength} символов.");
    }
}