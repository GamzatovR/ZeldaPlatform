using FluentValidation;

using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Application.Features.Account.Commands.ConfirmEmailChange;

public sealed class ConfirmEmailChangeCommandValidator
    : AbstractValidator<ConfirmEmailChangeCommand>
{
    public ConfirmEmailChangeCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("Ссылка подтверждения неполная.");

        RuleFor(command => command.Token)
            .NotEmpty()
            .WithMessage("Ссылка подтверждения неполная.");

        RuleFor(command => command.NewEmail).ValidAccountEmail();
    }
}