using FluentValidation;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Application.Features.Admin.Teams;

/// <summary>
/// Те же правила профиля команды, что и у сценариев капитана
/// (<c>EsportsValidationRules</c>): админка не должна принимать то, что отвергает
/// кабинет владельца, и наоборот.
/// </summary>
public sealed class TeamFieldsValidator : AbstractValidator<ITeamFields>
{
    public TeamFieldsValidator(IDateTimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        RuleFor(fields => fields.Name).ValidTeamName();
        RuleFor(fields => fields.Tag).ValidTag();
        RuleFor(fields => fields.CountryCode).ValidCountryCode();
        RuleFor(fields => fields.Description).ValidDescription();
        RuleFor(fields => fields.Logo).ValidImageUpload();

        RuleFor(fields => fields.Region)
            .IsInEnum()
            .WithMessage("Неизвестный регион.");

        RuleFor(fields => fields.FoundedAt)
            .Must(foundedAt => foundedAt is null || foundedAt <= clock.Today)
            .WithMessage("Дата основания не может быть в будущем.");
    }
}