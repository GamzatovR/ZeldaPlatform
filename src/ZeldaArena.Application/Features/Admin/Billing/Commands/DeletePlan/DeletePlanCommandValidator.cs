using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.DeletePlan;

public sealed class DeletePlanCommandValidator : AbstractValidator<DeletePlanCommand>
{
    public DeletePlanCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Не указан тариф.");
    }
}