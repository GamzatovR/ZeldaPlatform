using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.DeleteTeam;

public sealed class DeleteTeamCommandValidator : AbstractValidator<DeleteTeamCommand>
{
    public DeleteTeamCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Не указана команда.");
    }
}