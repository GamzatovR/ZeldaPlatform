using FluentValidation;

namespace ZeldaArena.Application.Features.Teams.Queries.GetTopTeams;

public sealed class GetTopTeamsQueryValidator : AbstractValidator<GetTopTeamsQuery>
{
    public const int MaxCount = 12;

    public GetTopTeamsQueryValidator()
    {
        RuleFor(query => query.Count).InclusiveBetween(1, MaxCount);
    }
}