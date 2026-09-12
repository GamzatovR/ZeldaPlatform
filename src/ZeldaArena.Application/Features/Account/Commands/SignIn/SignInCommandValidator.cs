using FluentValidation;

using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Application.Features.Account.Commands.SignIn;

public sealed class SignInCommandValidator : AbstractValidator<SignInCommand>
{
    public SignInCommandValidator()
    {
        RuleFor(command => command.Email).ValidAccountEmail();

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithMessage("Укажите пароль.");
    }
}