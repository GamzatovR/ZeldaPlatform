using FluentValidation;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Validation;

namespace ZeldaArena.Application.Features.Admin.Players;

/// <summary>
/// Те же правила, что у формы капитана при добавлении нового игрока в свою команду
/// (<c>EsportsValidationRules</c>): игрок один, и правила у него одни.
/// </summary>
public sealed class PlayerFieldsValidator : AbstractValidator<IPlayerFields>
{
    public const int MaxBioLength = 4000;

    /// <summary>Киберспортсмену не может быть меньше десяти лет — защита от опечатки в дате.</summary>
    public const int MinAgeYears = 10;

    public PlayerFieldsValidator(IDateTimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        RuleFor(fields => fields.Nickname).ValidNickname();
        RuleFor(fields => fields.CountryCode).ValidCountryCode();
        RuleFor(fields => fields.Avatar).ValidImageUpload();

        RuleFor(fields => fields.Role)
            .IsInEnum()
            .WithMessage("Неизвестная роль игрока.");

        RuleFor(fields => fields.FirstName)
            .MaximumLength(EsportsValidationRules.MaxPersonNameLength)
            .WithMessage($"Имя не длиннее {EsportsValidationRules.MaxPersonNameLength} символов.");

        RuleFor(fields => fields.LastName)
            .MaximumLength(EsportsValidationRules.MaxPersonNameLength)
            .WithMessage($"Фамилия не длиннее {EsportsValidationRules.MaxPersonNameLength} символов.");

        RuleFor(fields => fields.Bio)
            .MaximumLength(MaxBioLength)
            .WithMessage($"Биография не длиннее {MaxBioLength} символов.");

        RuleFor(fields => fields.BirthDate)
            .Must(date => date is null || date <= clock.Today.AddYears(-MinAgeYears))
            .WithMessage($"Дата рождения должна быть не позже, чем {MinAgeYears} лет назад.");
    }
}