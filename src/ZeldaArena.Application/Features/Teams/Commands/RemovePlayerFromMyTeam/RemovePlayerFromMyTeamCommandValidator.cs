using FluentValidation;

namespace ZeldaArena.Application.Features.Teams.Commands.RemovePlayerFromMyTeam;

public sealed class RemovePlayerFromMyTeamCommandValidator : AbstractValidator<RemovePlayerFromMyTeamCommand>
{
    public RemovePlayerFromMyTeamCommandValidator()
    {
        RuleFor(command => command.TeamId).NotEmpty();
        RuleFor(command => command.PlayerId).NotEmpty();
    }
}