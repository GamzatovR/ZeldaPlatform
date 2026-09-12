using System.ComponentModel.DataAnnotations;

using ZeldaArena.Application.Common.Validation;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Web.Validation;

namespace ZeldaArena.Web.Models.Teams;

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
    [NotInFuture(ErrorMessage = "Дата основания не может быть в будущем.")]
    [Display(Name = "Дата основания")]
    public DateOnly? FoundedAt { get; set; }

    [StringLength(EsportsValidationRules.MaxDescriptionLength, ErrorMessage = "Описание не длиннее 4000 символов.")]
    [Display(Name = "О команде")]
    public string? Description { get; set; }
}