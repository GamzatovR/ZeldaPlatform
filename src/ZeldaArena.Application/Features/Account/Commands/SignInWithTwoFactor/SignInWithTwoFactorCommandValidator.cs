using FluentValidation;

using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Application.Features.Account.Commands.SignInWithTwoFactor;

public sealed class SignInWithTwoFactorCommandValidator
    : AbstractValidator<SignInWithTwoFactorCommand>
{
    public SignInWithTwoFactorCommandValidator() =>
        RuleFor(command => command.Code).ValidTwoFactorCode();
}