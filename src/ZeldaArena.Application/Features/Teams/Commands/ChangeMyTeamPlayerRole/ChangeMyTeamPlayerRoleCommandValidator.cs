using FluentValidation;

namespace ZeldaArena.Application.Features.Teams.Commands.ChangeMyTeamPlayerRole;

public sealed class ChangeMyTeamPlayerRoleCommandValidator : AbstractValidator<ChangeMyTeamPlayerRoleCommand>
{
    public ChangeMyTeamPlayerRoleCommandValidator()
    {
        RuleFor(command => command.TeamId).NotEmpty();
        RuleFor(command => command.PlayerId).NotEmpty();

        RuleFor(command => command.Role)
            .IsInEnum()
            .WithMessage("Неизвестная роль игрока.");
    }
}