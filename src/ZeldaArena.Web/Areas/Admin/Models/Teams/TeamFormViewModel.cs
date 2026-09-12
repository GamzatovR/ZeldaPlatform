using System.ComponentModel.DataAnnotations;

using ZeldaArena.Application.Common.Validation;
using ZeldaArena.Application.Features.Admin.Teams;
using ZeldaArena.Application.Features.Admin.Teams.Queries.GetTeamForEdit;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Web.Validation;

namespace ZeldaArena.Web.Areas.Admin.Models.Teams;

public sealed class TeamFormViewModel
{
    [Required(ErrorMessage = "Укажите название команды.")]
    [StringLength(EsportsValidationRules.MaxTeamNameLength, ErrorMessage = "Название не длиннее {1} символов.")]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите тег команды.")]
    [RegularExpression(EsportsValidationRules.TagPattern, ErrorMessage = "Тег — от 2 до 8 латинских букв или цифр.")]
    [Display(Name = "Тег")]
    public string Tag { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите страну.")]
    [RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "Код страны — две латинские буквы ISO 3166, например RU.")]
    [Display(Name = "Страна (код ISO)")]
    public string CountryCode { get; set; } = "RU";

    [Display(Name = "Регион")]
    public Region Region { get; set; } = Region.Cis;

    [DataType(DataType.Date)]
    [NotInFuture(ErrorMessage = "Дата основания не может быть в будущем.")]
    [Display(Name = "Дата основания")]
    public DateOnly? FoundedAt { get; set; }

    [StringLength(EsportsValidationRules.MaxDescriptionLength, ErrorMessage = "Описание не длиннее {1} символов.")]
    [Display(Name = "О команде")]
    public string? Description { get; set; }

    [Range(0, TeamRatingRules.MaxRating, ErrorMessage = "Рейтинг — от {1} до {2}.")]
    [Display(Name = "Рейтинг")]
    public int Rating { get; set; }

    [ImageFile]
    [Display(Name = "Логотип")]
    public IFormFile? Logo { get; set; }

    [Display(Name = "Убрать логотип")]
    public bool RemoveLogo { get; set; }

    /// <summary>Текущий логотип — только для показа на форме правки.</summary>
    public string? CurrentLogoPath { get; set; }

    public static TeamFormViewModel From(TeamEditDto team)
    {
        ArgumentNullException.ThrowIfNull(team);

        return new TeamFormViewModel
        {
            Name = team.Name,
            Tag = team.Tag,
            CountryCode = team.CountryCode,
            Region = team.Region,
            FoundedAt = team.FoundedAt,
            Description = team.Description,
            Rating = team.Rating,
            CurrentLogoPath = team.LogoPath,
        };
    }
}