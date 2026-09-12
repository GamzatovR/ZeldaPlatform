using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.AddTeamPlayer;

public sealed class AddTeamPlayerCommandValidator : AbstractValidator<AddTeamPlayerCommand>
{
    public AddTeamPlayerCommandValidator()
    {
        RuleFor(command => command.TeamId).NotEmpty().WithMessage("Не указана команда.");
        RuleFor(command => command.PlayerId).NotEmpty().WithMessage("Выберите игрока.");
        RuleFor(command => command.Role).IsInEnum().WithMessage("Неизвестная роль игрока.");
    }
}