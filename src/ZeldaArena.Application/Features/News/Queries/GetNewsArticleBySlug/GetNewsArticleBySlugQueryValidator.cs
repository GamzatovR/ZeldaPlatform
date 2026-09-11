using FluentValidation;

using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.News.Queries.GetNewsArticleBySlug;

public sealed class GetNewsArticleBySlugQueryValidator : AbstractValidator<GetNewsArticleBySlugQuery>
{
    public GetNewsArticleBySlugQueryValidator()
    {
        RuleFor(query => query.Slug)
            .NotEmpty()
            .MaximumLength(Slug.MaxLength);
    }
}