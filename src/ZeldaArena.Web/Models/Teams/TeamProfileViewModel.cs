using System.ComponentModel.DataAnnotations;

using ZeldaArena.Application.Common.Validation;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Web.Models.Teams;

/// <summary>
/// Поля профиля команды — клиентский уровень двухуровневой валидации (docs/SPEC.md §15).
/// Длины и шаблоны взяты из <see cref="EsportsValidationRules"/>, тех же, что читают
/// FluentValidation-валидаторы создания и правки: два уровня проверяют одно и то же.
/// </summary>
public class TeamProfileViewModel
{
    [Required(ErrorMessage = "Укажите название команды.")]
    [StringLength(EsportsValidationRules.MaxTeamNameLength, ErrorMessage = "Название не длиннее 120 символов.")]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите тег команды.")]
    [RegularExpression(EsportsValidationRules.TagPattern, ErrorMessage = "Тег — от 2 до 8 латинских букв или цифр.")]
    [Display(Name = "Тег")]
    public string Tag { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите страну.")]
    [RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "Код страны — две латинские буквы ISO 3166, например RU.")]
    [Display(Name = "Страна (код ISO, например RU)")]
    public string CountryCode { get; set; } = "RU";

    [Required(ErrorMessage = "Выберите регион.")]
    [Display(Name = "Регион")]
    public Region Region { get; set; } = Region.Cis;

    [DataType(DataType.Date)]
    [Display(Name = "Дата основания")]
    public DateOnly? FoundedAt { get; set; }

    [StringLength(EsportsValidationRules.MaxDescriptionLength, ErrorMessage = "Описание не длиннее 4000 символов.")]
    [Display(Name = "О команде")]
    public string? Description { get; set; }
}