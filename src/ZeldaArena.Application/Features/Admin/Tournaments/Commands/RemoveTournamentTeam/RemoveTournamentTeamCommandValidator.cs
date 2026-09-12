using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.RemoveTournamentTeam;

public sealed class RemoveTournamentTeamCommandValidator : AbstractValidator<RemoveTournamentTeamCommand>
{
    public RemoveTournamentTeamCommandValidator()
    {
        RuleFor(command => command.TournamentId).NotEmpty().WithMessage("Не указан турнир.");
        RuleFor(command => command.TeamId).NotEmpty().WithMessage("Не указана команда.");
    }
}