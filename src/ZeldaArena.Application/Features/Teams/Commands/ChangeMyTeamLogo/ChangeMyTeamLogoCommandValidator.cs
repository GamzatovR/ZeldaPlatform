using FluentValidation;

using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Application.Features.Teams.Commands.ChangeMyTeamLogo;

public sealed class ChangeMyTeamLogoCommandValidator : AbstractValidator<ChangeMyTeamLogoCommand>
{
    public ChangeMyTeamLogoCommandValidator()
    {
        RuleFor(command => command.TeamId).NotEmpty();

        RuleFor(command => command.Logo)
            .NotNull()
            .WithMessage("Выберите файл логотипа.")
            .ValidImageUpload();
    }
}