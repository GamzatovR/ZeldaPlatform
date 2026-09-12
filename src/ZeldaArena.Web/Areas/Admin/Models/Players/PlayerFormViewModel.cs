using System.ComponentModel.DataAnnotations;

using ZeldaArena.Application.Common.Validation;
using ZeldaArena.Application.Features.Admin.Players;
using ZeldaArena.Application.Features.Admin.Players.Queries.GetPlayerForEdit;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Web.Validation;

namespace ZeldaArena.Web.Areas.Admin.Models.Players;

public sealed class PlayerFormViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Укажите ник игрока.")]
    [StringLength(EsportsValidationRules.MaxNicknameLength, ErrorMessage = "Ник не длиннее {1} символов.")]
    [Display(Name = "Ник")]
    public string Nickname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите страну.")]
    [RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "Код страны — две латинские буквы ISO 3166, например RU.")]
    [Display(Name = "Страна (код ISO)")]
    public string CountryCode { get; set; } = "RU";

    [Display(Name = "Роль")]
    public PlayerRole Role { get; set; } = PlayerRole.Attacker;

    [StringLength(EsportsValidationRules.MaxPersonNameLength, ErrorMessage = "Имя не длиннее {1} символов.")]
    [Display(Name = "Имя")]
    public string? FirstName { get; set; }

    [StringLength(EsportsValidationRules.MaxPersonNameLength, ErrorMessage = "Фамилия не длиннее {1} символов.")]
    [Display(Name = "Фамилия")]
    public string? LastName { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Дата рождения")]
    public DateOnly? BirthDate { get; set; }

    [StringLength(PlayerFieldsValidator.MaxBioLength, ErrorMessage = "Биография не длиннее {1} символов.")]
    [Display(Name = "Биография")]
    public string? Bio { get; set; }

    [ImageFile]
    [Display(Name = "Аватар")]
    public IFormFile? Avatar { get; set; }

    [Display(Name = "Убрать аватар")]
    public bool RemoveAvatar { get; set; }

    public string? CurrentAvatarPath { get; set; }

    /// <summary>Самая поздняя допустимая дата рождения: младше игроков не бывает.</summary>
    public static DateOnly LatestBirthDate =>
        DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-PlayerFieldsValidator.MinAgeYears);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.IsDefined(Role))
        {
            yield return new ValidationResult("Неизвестная роль игрока.", [nameof(Role)]);
        }

        // Атрибут max в разметке браузер уважает, но форма приходит и в обход него.
        if (BirthDate is { } birthDate && birthDate > LatestBirthDate)
        {
            yield return new ValidationResult(
                $"Дата рождения должна быть не позже, чем {PlayerFieldsValidator.MinAgeYears} лет назад.",
                [nameof(BirthDate)]);
        }
    }

    public static PlayerFormViewModel From(PlayerEditDto player)
    {
        ArgumentNullException.ThrowIfNull(player);

        return new PlayerFormViewModel
        {
            Nickname = player.Nickname,
            CountryCode = player.CountryCode,
            Role = player.Role,
            FirstName = player.FirstName,
            LastName = player.LastName,
            BirthDate = player.BirthDate,
            Bio = player.Bio,
            CurrentAvatarPath = player.AvatarPath,
        };
    }
}