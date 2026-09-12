using FluentValidation;

namespace ZeldaArena.Application.Features.Payments.Commands.StartSubscriptionPayment;

public sealed class StartSubscriptionPaymentCommandValidator
    : AbstractValidator<StartSubscriptionPaymentCommand>
{
    public StartSubscriptionPaymentCommandValidator()
    {
        RuleFor(command => command.PlanId)
            .NotEmpty()
            .WithMessage("Выберите тариф.");

        this.AddCardPaymentRules();
    }
}