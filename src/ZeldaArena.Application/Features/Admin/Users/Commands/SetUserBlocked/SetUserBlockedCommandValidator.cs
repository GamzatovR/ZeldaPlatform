using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Users.Commands.SetUserBlocked;

public sealed class SetUserBlockedCommandValidator : AbstractValidator<SetUserBlockedCommand>
{
    public SetUserBlockedCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty().WithMessage("Не указан пользователь.");
    }
}