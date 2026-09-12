using FluentValidation;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.SetTeamApproval;

public sealed class SetTeamApprovalCommandValidator : AbstractValidator<SetTeamApprovalCommand>
{
    public SetTeamApprovalCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Не указана команда.");
    }
}