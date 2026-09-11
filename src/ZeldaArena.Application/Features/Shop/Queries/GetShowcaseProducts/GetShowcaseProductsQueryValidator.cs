using FluentValidation;

namespace ZeldaArena.Application.Features.Shop.Queries.GetShowcaseProducts;

public sealed class GetShowcaseProductsQueryValidator : AbstractValidator<GetShowcaseProductsQuery>
{
    public const int MaxCount = 12;

    public GetShowcaseProductsQueryValidator()
    {
        RuleFor(query => query.Count).InclusiveBetween(1, MaxCount);
    }
}