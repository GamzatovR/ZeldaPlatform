using FluentValidation;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Application.Features.Teams.Commands.UpdateMyTeamProfile;

/// <summary>Те же правила полей, что при создании команды (<see cref="EsportsValidationRules"/>).</summary>
public sealed class UpdateMyTeamProfileCommandValidator : AbstractValidator<UpdateMyTeamProfileCommand>
{
    public UpdateMyTeamProfileCommandValidator(IDateTimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        RuleFor(command => command.TeamId).NotEmpty();
        RuleFor(command => command.Name).ValidTeamName();
        RuleFor(command => command.Tag).ValidTag();
        RuleFor(command => command.CountryCode).ValidCountryCode();
        RuleFor(command => command.Description).ValidDescription();

        RuleFor(command => command.Region)
            .IsInEnum()
            .WithMessage("Неизвестный регион.");

        RuleFor(command => command.FoundedAt)
            .Must(foundedAt => foundedAt is null || foundedAt <= clock.Today)
            .WithMessage("Дата основания не может быть в будущем.");
    }
}