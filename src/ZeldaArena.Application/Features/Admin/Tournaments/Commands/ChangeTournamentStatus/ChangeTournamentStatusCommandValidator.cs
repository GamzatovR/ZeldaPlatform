using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.ChangeTournamentStatus;

public sealed class ChangeTournamentStatusCommandValidator : AbstractValidator<ChangeTournamentStatusCommand>
{
    public ChangeTournamentStatusCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Не указан турнир.");
        RuleFor(command => command.Transition).IsInEnum().WithMessage("Неизвестное действие со статусом.");
    }
}