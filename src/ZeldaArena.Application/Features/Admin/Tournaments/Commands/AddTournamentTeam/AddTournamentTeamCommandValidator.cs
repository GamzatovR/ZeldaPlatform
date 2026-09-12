using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.AddTournamentTeam;

public sealed class AddTournamentTeamCommandValidator : AbstractValidator<AddTournamentTeamCommand>
{
    /// <summary>Посев — место в сетке; сетки больше 256 команд в проекте нет.</summary>
    public const int MaxSeed = 256;

    public AddTournamentTeamCommandValidator()
    {
        RuleFor(command => command.TournamentId).NotEmpty().WithMessage("Не указан турнир.");
        RuleFor(command => command.TeamId).NotEmpty().WithMessage("Выберите команду.");
        RuleFor(command => command.Seed)
            .InclusiveBetween(1, MaxSeed)
            .WithMessage($"Посев — от 1 до {MaxSeed}.");
    }
}