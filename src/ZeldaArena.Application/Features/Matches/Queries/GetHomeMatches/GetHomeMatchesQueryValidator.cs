using FluentValidation;

namespace ZeldaArena.Application.Features.Matches.Queries.GetHomeMatches;

public sealed class GetHomeMatchesQueryValidator : AbstractValidator<GetHomeMatchesQuery>
{
    public const int MaxCount = 12;

    public GetHomeMatchesQueryValidator()
    {
        RuleFor(query => query.Count)
            .InclusiveBetween(1, MaxCount);
    }
}