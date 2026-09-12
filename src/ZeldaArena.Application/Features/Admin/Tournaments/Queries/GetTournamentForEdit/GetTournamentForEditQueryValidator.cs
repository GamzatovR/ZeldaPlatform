using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentForEdit;

public sealed class GetTournamentForEditQueryValidator : AbstractValidator<GetTournamentForEditQuery>
{
    public GetTournamentForEditQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty().WithMessage("Не указан турнир.");
    }
}