using FluentValidation;

using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Application.Features.Account.Commands.SignIn;

/// <summary>
/// На входе проверяется только заполненность: требования к паролю здесь не к месту.
/// Старый пароль мог быть заведён до ужесточения правил, и отказ формы «пароль
/// слишком короткий» не дал бы такому пользователю войти и сменить его.
/// </summary>
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