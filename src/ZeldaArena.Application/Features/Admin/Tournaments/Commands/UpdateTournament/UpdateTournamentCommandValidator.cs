using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.UpdateTournament;

public sealed class UpdateTournamentCommandValidator : AbstractValidator<UpdateTournamentCommand>
{
    public UpdateTournamentCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Не указан турнир.");

        Include(new TournamentFieldsValidator());
    }
}