using FluentValidation;

namespace ZeldaArena.Application.Features.Payments.Commands.StartSubscriptionPayment;

/// <summary>
/// Тариф плюс общие правила реквизитов (<see cref="CardPaymentRules"/>): те же самые,
/// что у оплаты заказа.
/// </summary>
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