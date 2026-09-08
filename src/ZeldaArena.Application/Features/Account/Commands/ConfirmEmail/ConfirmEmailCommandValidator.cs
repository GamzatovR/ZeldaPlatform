using FluentValidation;

namespace ZeldaArena.Application.Features.Account.Commands.ConfirmEmail;

/// <summary>
/// Проверяет только наличие параметров: обрезанная ссылка из письма должна дать
/// понятный отказ, а не исключение. Годность самого токена определяет Identity.
/// </summary>
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