using FluentValidation;

using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Application.Features.Payments.Commands.ConfirmPayment;

public sealed class ConfirmPaymentCommandValidator : AbstractValidator<ConfirmPaymentCommand>
{
    public ConfirmPaymentCommandValidator()
    {
        RuleFor(command => command.PaymentId)
            .NotEmpty()
            .WithMessage("Платёж не указан.");

        RuleFor(command => command.ConfirmationCode)
            .NotEmpty()
            .WithMessage("Введите код из письма.")
            .Must(IsSixDigits)
            .WithMessage($"Код состоит из {PaymentPolicy.CodeLength} цифр.");
    }

    private static bool IsSixDigits(string? code) =>
        code is not null
        && code.Trim().Length == PaymentPolicy.CodeLength
        && code.Trim().All(char.IsAsciiDigit);
}