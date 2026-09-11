using FluentValidation;

namespace ZeldaArena.Application.Features.Players.Queries.GetPlayerAdvancedStats;

public sealed class GetPlayerAdvancedStatsQueryValidator : AbstractValidator<GetPlayerAdvancedStatsQuery>
{
    public GetPlayerAdvancedStatsQueryValidator()
    {
        RuleFor(query => query.PlayerId).NotEmpty();
    }
}