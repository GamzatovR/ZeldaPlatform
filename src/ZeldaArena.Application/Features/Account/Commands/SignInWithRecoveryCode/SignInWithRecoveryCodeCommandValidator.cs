using FluentValidation;

using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Application.Features.Account.Commands.SignInWithRecoveryCode;

public sealed class SignInWithRecoveryCodeCommandValidator
    : AbstractValidator<SignInWithRecoveryCodeCommand>
{
    public SignInWithRecoveryCodeCommandValidator() =>
        RuleFor(command => command.RecoveryCode).ValidRecoveryCode();
}