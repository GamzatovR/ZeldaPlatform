using FluentValidation;

namespace ZeldaArena.Application.Features.Teams.Queries.GetTeamAdvancedStats;

public sealed class GetTeamAdvancedStatsQueryValidator : AbstractValidator<GetTeamAdvancedStatsQuery>
{
    public GetTeamAdvancedStatsQueryValidator()
    {
        RuleFor(query => query.TeamId).NotEmpty();
    }
}