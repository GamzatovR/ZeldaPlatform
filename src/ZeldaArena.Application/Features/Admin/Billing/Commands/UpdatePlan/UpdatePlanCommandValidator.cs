using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.UpdatePlan;

public sealed class UpdatePlanCommandValidator : AbstractValidator<UpdatePlanCommand>
{
    public UpdatePlanCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Не указан тариф.");
        RuleFor(command => command.Name).ValidPlanName();
        RuleFor(command => command.Description).ValidPlanDescription();
        RuleFor(command => command.Price).ValidPlanPrice();
        RuleFor(command => command.DurationDays).ValidPlanDuration();
    }
}