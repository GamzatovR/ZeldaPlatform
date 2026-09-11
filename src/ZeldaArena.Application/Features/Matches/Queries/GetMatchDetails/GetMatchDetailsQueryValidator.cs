using FluentValidation;

namespace ZeldaArena.Application.Features.Matches.Queries.GetMatchDetails;

public sealed class GetMatchDetailsQueryValidator : AbstractValidator<GetMatchDetailsQuery>
{
    public GetMatchDetailsQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}