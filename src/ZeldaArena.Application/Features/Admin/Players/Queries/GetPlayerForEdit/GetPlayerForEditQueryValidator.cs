using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Players.Queries.GetPlayerForEdit;

public sealed class GetPlayerForEditQueryValidator : AbstractValidator<GetPlayerForEditQuery>
{
    public GetPlayerForEditQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty().WithMessage("Не указан игрок.");
    }
}