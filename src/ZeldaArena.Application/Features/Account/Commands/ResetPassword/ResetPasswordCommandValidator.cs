using FluentValidation;

using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Application.Features.Account.Commands.ResetPassword;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("Ссылка восстановления неполная.");

        RuleFor(command => command.Token)
            .NotEmpty()
            .WithMessage("Ссылка восстановления неполная.");

        RuleFor(command => command.NewPassword).ValidPassword();
    }
}