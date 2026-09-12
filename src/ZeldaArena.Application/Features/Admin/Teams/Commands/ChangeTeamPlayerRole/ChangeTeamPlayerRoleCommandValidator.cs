using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.ChangeTeamPlayerRole;

public sealed class ChangeTeamPlayerRoleCommandValidator : AbstractValidator<ChangeTeamPlayerRoleCommand>
{
    public ChangeTeamPlayerRoleCommandValidator()
    {
        RuleFor(command => command.TeamId).NotEmpty().WithMessage("Не указана команда.");
        RuleFor(command => command.PlayerId).NotEmpty().WithMessage("Не указан игрок.");
        RuleFor(command => command.Role).IsInEnum().WithMessage("Неизвестная роль игрока.");
    }
}