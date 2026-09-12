using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Commands.CreateTournament;

public sealed class CreateTournamentCommandValidator : AbstractValidator<CreateTournamentCommand>
{
    public CreateTournamentCommandValidator()
    {
        Include(new TournamentFieldsValidator());
    }
}