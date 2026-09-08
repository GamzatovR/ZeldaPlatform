using FluentValidation;

using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Application.Features.Account.Commands.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(command => command.CurrentPassword)
            .NotEmpty()
            .WithMessage("Укажите текущий пароль.");

        RuleFor(command => command.NewPassword).ValidPassword();

        RuleFor(command => command.NewPassword)
            .NotEqual(command => command.CurrentPassword)
            .WithMessage("Новый пароль должен отличаться от текущего.");
    }
}