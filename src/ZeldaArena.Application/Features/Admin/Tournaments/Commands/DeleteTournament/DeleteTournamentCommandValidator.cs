using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.DeleteTournament;

public sealed class DeleteTournamentCommandValidator : AbstractValidator<DeleteTournamentCommand>
{
    public DeleteTournamentCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Не указан турнир.");
    }
}