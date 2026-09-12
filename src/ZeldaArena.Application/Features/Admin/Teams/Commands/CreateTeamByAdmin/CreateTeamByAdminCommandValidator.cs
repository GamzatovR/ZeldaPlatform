using FluentValidation;

using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Application.Features.Admin.Teams.Commands.CreateTeamByAdmin;

public sealed class CreateTeamByAdminCommandValidator : AbstractValidator<CreateTeamByAdminCommand>
{
    public CreateTeamByAdminCommandValidator(IDateTimeProvider clock)
    {
        Include(new TeamFieldsValidator(clock));

        RuleFor(command => command.Rating).ValidRating();
    }
}