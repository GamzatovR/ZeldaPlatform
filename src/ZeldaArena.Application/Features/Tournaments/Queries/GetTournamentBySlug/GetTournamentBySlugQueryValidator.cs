using FluentValidation;

namespace ZeldaArena.Application.Features.Tournaments.Queries.GetTournamentBySlug;

public sealed class GetTournamentBySlugQueryValidator : AbstractValidator<GetTournamentBySlugQuery>
{
    public GetTournamentBySlugQueryValidator()
    {
        RuleFor(query => query.Slug).NotEmpty();
    }
}