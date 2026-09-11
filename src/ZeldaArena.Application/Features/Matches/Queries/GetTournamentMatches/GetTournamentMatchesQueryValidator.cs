using FluentValidation;

namespace ZeldaArena.Application.Features.Matches.Queries.GetTournamentMatches;

public sealed class GetTournamentMatchesQueryValidator : AbstractValidator<GetTournamentMatchesQuery>
{
    public GetTournamentMatchesQueryValidator()
    {
        RuleFor(query => query.TournamentId).NotEmpty();

        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Номер страницы начинается с единицы.");

        RuleFor(query => query.State)
            .IsInEnum()
            .WithMessage("Неизвестная вкладка матчей.");
    }
}