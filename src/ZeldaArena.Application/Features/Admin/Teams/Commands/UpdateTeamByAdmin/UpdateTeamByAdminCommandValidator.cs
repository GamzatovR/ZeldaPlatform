using FluentValidation;

using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.UpdateTeamByAdmin;

public sealed class UpdateTeamByAdminCommandValidator : AbstractValidator<UpdateTeamByAdminCommand>
{
    public UpdateTeamByAdminCommandValidator(IDateTimeProvider clock)
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Не указана команда.");

        Include(new TeamFieldsValidator(clock));

        RuleFor(command => command.Rating).ValidRating();
    }
}