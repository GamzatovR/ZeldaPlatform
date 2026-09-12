using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Shop.Queries.GetProductForEdit;

public sealed class GetProductForEditQueryValidator : AbstractValidator<GetProductForEditQuery>
{
    public GetProductForEditQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty().WithMessage("Не указан товар.");
    }
}