using FluentValidation;

using ZeldaArena.Application.Features.Admin.Tournaments.Commands.AddTournamentTeam;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.UpdateTournamentTeam;

public sealed class UpdateTournamentTeamCommandValidator : AbstractValidator<UpdateTournamentTeamCommand>
{
    public UpdateTournamentTeamCommandValidator()
    {
        RuleFor(command => command.TournamentId).NotEmpty().WithMessage("Не указан турнир.");
        RuleFor(command => command.TeamId).NotEmpty().WithMessage("Не указана команда.");

        RuleFor(command => command.Seed)
            .InclusiveBetween(1, AddTournamentTeamCommandValidator.MaxSeed)
            .WithMessage($"Посев — от 1 до {AddTournamentTeamCommandValidator.MaxSeed}.");

        RuleFor(command => command.Placement)
            .InclusiveBetween(1, AddTournamentTeamCommandValidator.MaxSeed)
            .When(command => command.Placement.HasValue)
            .WithMessage($"Место — от 1 до {AddTournamentTeamCommandValidator.MaxSeed}.");
    }
}