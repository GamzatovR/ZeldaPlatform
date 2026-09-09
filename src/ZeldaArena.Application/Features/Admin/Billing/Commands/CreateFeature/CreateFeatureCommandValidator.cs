using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.CreateFeature;

public sealed class CreateFeatureCommandValidator : AbstractValidator<CreateFeatureCommand>
{
    public const int MaxCodeLength = 64;
    public const int MaxNameLength = 128;
    public const int MaxDescriptionLength = 512;

    public CreateFeatureCommandValidator()
    {
        // Код попадает в имя политики Feature:{code} и в адрес ?required=,
        // поэтому набор символов ограничен намеренно.
        RuleFor(command => command.Code)
            .NotEmpty()
            .WithMessage("Укажите код функции.")
            .MaximumLength(MaxCodeLength)
            .WithMessage($"Код не длиннее {MaxCodeLength} символов.")
            .Matches(@"^[a-z][a-z0-9]*(\.[a-z0-9]+)*$")
            .WithMessage("Код состоит из латинских букв, цифр и точек, например team.create.");

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