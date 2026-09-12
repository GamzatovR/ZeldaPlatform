using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Teams.Queries.GetTeamForEdit;

public sealed class GetTeamForEditQueryValidator : AbstractValidator<GetTeamForEditQuery>
{
    public GetTeamForEditQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty().WithMessage("Не указана команда.");
    }
}