using FluentValidation;

namespace ZeldaArena.Application.Features.Teams.Commands.AddFreeAgentToMyTeam;

public sealed class AddFreeAgentToMyTeamCommandValidator : AbstractValidator<AddFreeAgentToMyTeamCommand>
{
    public AddFreeAgentToMyTeamCommandValidator()
    {
        RuleFor(command => command.TeamId).NotEmpty();

        RuleFor(command => command.PlayerId)
            .NotEmpty()
            .WithMessage("Выберите игрока.");

        RuleFor(command => command.Role)
            .IsInEnum()
            .WithMessage("Неизвестная роль игрока.");
    }
}