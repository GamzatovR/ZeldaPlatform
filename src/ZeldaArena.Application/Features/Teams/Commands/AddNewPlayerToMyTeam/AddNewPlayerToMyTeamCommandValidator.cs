using FluentValidation;

using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Application.Features.Teams.Commands.AddNewPlayerToMyTeam;

public sealed class AddNewPlayerToMyTeamCommandValidator : AbstractValidator<AddNewPlayerToMyTeamCommand>
{
    public AddNewPlayerToMyTeamCommandValidator()
    {
        RuleFor(command => command.TeamId).NotEmpty();
        RuleFor(command => command.Nickname).ValidNickname();
        RuleFor(command => command.CountryCode).ValidCountryCode();

        RuleFor(command => command.Role)
            .IsInEnum()
            .WithMessage("Неизвестная роль игрока.");

        RuleFor(command => command.FirstName)
            .MaximumLength(EsportsValidationRules.MaxPersonNameLength)
            .WithMessage($"Имя не длиннее {EsportsValidationRules.MaxPersonNameLength} символов.");

        RuleFor(command => command.LastName)
            .MaximumLength(EsportsValidationRules.MaxPersonNameLength)
            .WithMessage($"Фамилия не длиннее {EsportsValidationRules.MaxPersonNameLength} символов.");
    }
}