using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.RemoveTeamPlayer;

public sealed class RemoveTeamPlayerCommandValidator : AbstractValidator<RemoveTeamPlayerCommand>
{
    public RemoveTeamPlayerCommandValidator()
    {
        RuleFor(command => command.TeamId).NotEmpty().WithMessage("Не указана команда.");
        RuleFor(command => command.PlayerId).NotEmpty().WithMessage("Не указан игрок.");
    }
}