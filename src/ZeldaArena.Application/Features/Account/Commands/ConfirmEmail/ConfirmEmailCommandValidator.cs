using FluentValidation;

namespace ZeldaArena.Application.Features.Account.Commands.ConfirmEmail;

public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("Ссылка подтверждения неполная.");

        RuleFor(command => command.Token)
            .NotEmpty()
            .WithMessage("Ссылка подтверждения неполная.");
    }
}