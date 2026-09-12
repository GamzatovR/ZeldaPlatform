using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Billing.Commands.CreatePlan;

public sealed class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanCommandValidator()
    {
        RuleFor(command => command.Code).ValidPlanCode();
        RuleFor(command => command.Name).ValidPlanName();
        RuleFor(command => command.Description).ValidPlanDescription();
        RuleFor(command => command.Price).ValidPlanPrice();
        RuleFor(command => command.DurationDays).ValidPlanDuration();
    }
}