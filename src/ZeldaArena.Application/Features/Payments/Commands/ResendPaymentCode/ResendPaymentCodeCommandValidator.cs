using FluentValidation;

namespace ZeldaArena.Application.Features.Payments.Commands.ResendPaymentCode;

public sealed class ResendPaymentCodeCommandValidator : AbstractValidator<ResendPaymentCodeCommand>
{
    public ResendPaymentCodeCommandValidator() =>
        RuleFor(command => command.PaymentId)
            .NotEmpty()
            .WithMessage("Платёж не указан.");
}