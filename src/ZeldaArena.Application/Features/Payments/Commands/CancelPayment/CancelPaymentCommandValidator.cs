using FluentValidation;

namespace ZeldaArena.Application.Features.Payments.Commands.CancelPayment;

public sealed class CancelPaymentCommandValidator : AbstractValidator<CancelPaymentCommand>
{
    public CancelPaymentCommandValidator() =>
        RuleFor(command => command.PaymentId)
            .NotEmpty()
            .WithMessage("Платёж не указан.");
}