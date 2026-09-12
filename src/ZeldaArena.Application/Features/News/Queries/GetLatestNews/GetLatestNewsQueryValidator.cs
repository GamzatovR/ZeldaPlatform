using FluentValidation;

namespace ZeldaArena.Application.Features.News.Queries.GetLatestNews;

public sealed class GetLatestNewsQueryValidator : AbstractValidator<GetLatestNewsQuery>
{
    public const int MaxCount = 12;

    public GetLatestNewsQueryValidator()
    {
        RuleFor(query => query.Count)
            .InclusiveBetween(1, MaxCount);
    }
}