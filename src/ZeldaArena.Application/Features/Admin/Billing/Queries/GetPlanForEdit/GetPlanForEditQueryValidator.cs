using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Billing.Queries.GetPlanForEdit;

public sealed class GetPlanForEditQueryValidator : AbstractValidator<GetPlanForEditQuery>
{
    public GetPlanForEditQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty().WithMessage("Не указан тариф.");
    }
}