using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Users.Commands.SetUserRoles;

public sealed class SetUserRolesCommandValidator : AbstractValidator<SetUserRolesCommand>
{
    public SetUserRolesCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty().WithMessage("Не указан пользователь.");
        RuleFor(command => command.Roles).NotNull();
    }
}