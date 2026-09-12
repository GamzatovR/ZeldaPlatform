using System.ComponentModel.DataAnnotations;

using ZeldaArena.Application.Common.Validation;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Web.Models.MyTeam;

/// <summary>Новый игрок в состав: клиентский уровень валидации.</summary>
public sealed class NewPlayerViewModel
{
    [Required(ErrorMessage = "Укажите ник игрока.")]
    [StringLength(EsportsValidationRules.MaxNicknameLength, ErrorMessage = "Ник не длиннее 60 символов.")]
    [Display(Name = "Ник")]
    public string Nickname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите роль.")]
    [Display(Name = "Роль")]
    public PlayerRole Role { get; set; } = PlayerRole.Attacker;

    [Required(ErrorMessage = "Укажите страну.")]
    [RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "Код страны — две латинские буквы ISO 3166, например RU.")]
    [Display(Name = "Страна (код ISO)")]
    public string CountryCode { get; set; } = "RU";

    [StringLength(EsportsValidationRules.MaxPersonNameLength, ErrorMessage = "Имя не длиннее 80 символов.")]
    [Display(Name = "Имя")]
    public string? FirstName { get; set; }

    [StringLength(EsportsValidationRules.MaxPersonNameLength, ErrorMessage = "Фамилия не длиннее 80 символов.")]
    [Display(Name = "Фамилия")]
    public string? LastName { get; set; }
}