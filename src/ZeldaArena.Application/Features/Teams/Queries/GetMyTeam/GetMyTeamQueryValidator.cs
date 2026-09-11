using FluentValidation;

namespace ZeldaArena.Application.Features.Teams.Queries.GetMyTeam;

public sealed class GetMyTeamQueryValidator : AbstractValidator<GetMyTeamQuery>
{
    public GetMyTeamQueryValidator()
    {
        RuleFor(query => query.TeamId)
            .NotEqual(Guid.Empty)
            .When(query => query.TeamId.HasValue);
    }
}