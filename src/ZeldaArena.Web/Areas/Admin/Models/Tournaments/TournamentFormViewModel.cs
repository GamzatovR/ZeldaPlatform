using System.ComponentModel.DataAnnotations;

using ZeldaArena.Application.Features.Admin.Tournaments;
using ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentForEdit;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Web.Validation;

namespace ZeldaArena.Web.Areas.Admin.Models.Tournaments;

public sealed class TournamentFormViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Укажите название турнира.")]
    [StringLength(TournamentFieldsValidator.MaxNameLength, ErrorMessage = "Название не длиннее {1} символов.")]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Уровень")]
    public TournamentTier Tier { get; set; } = TournamentTier.B;

    [Display(Name = "Регион")]
    public Region Region { get; set; } = Region.Europe;

    [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Призовой фонд — от нуля до миллиарда.")]
    [Display(Name = "Призовой фонд, ₽")]
    public decimal PrizePool { get; set; }

    [Required(ErrorMessage = "Укажите начало турнира.")]
    [Display(Name = "Начало (UTC)")]
    public DateTime StartsAt { get; set; }

    [Required(ErrorMessage = "Укажите окончание турнира.")]
    [Display(Name = "Окончание (UTC)")]
    public DateTime EndsAt { get; set; }

    [StringLength(TournamentFieldsValidator.MaxDescriptionLength, ErrorMessage = "Описание не длиннее {1} символов.")]
    [Display(Name = "Описание")]
    public string? Description { get; set; }

    [StringLength(TournamentFieldsValidator.MaxRulesLength, ErrorMessage = "Регламент не длиннее {1} символов.")]
    [Display(Name = "Регламент (HTML)")]
    public string? RulesHtml { get; set; }

    [Display(Name = "Показывать в избранном на главной")]
    public bool IsFeatured { get; set; }

    [ImageFile]
    [Display(Name = "Логотип")]
    public IFormFile? Logo { get; set; }

    [Display(Name = "Убрать логотип")]
    public bool RemoveLogo { get; set; }

    /// <summary>Текущий логотип — только для показа на форме правки, из запроса не берётся.</summary>
    public string? CurrentLogoPath { get; set; }

    public static TournamentFormViewModel From(TournamentEditDto tournament)
    {
        ArgumentNullException.ThrowIfNull(tournament);

        return new TournamentFormViewModel
        {
            Name = tournament.Name,
            Tier = tournament.Tier,
            Region = tournament.Region,
            PrizePool = tournament.PrizePool,
            StartsAt = UtcInput.FromUtc(tournament.StartsAt),
            EndsAt = UtcInput.FromUtc(tournament.EndsAt),
            Description = tournament.Description,
            RulesHtml = tournament.RulesHtml,
            IsFeatured = tournament.IsFeatured,
            CurrentLogoPath = tournament.LogoPath,
        };
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.IsDefined(Tier))
        {
            yield return new ValidationResult("Неизвестный уровень турнира.", [nameof(Tier)]);
        }

        if (!Enum.IsDefined(Region))
        {
            yield return new ValidationResult("Неизвестный регион.", [nameof(Region)]);
        }

        if (EndsAt < StartsAt)
        {
            yield return new ValidationResult("Турнир не может закончиться раньше, чем начнётся.", [nameof(EndsAt)]);
        }
    }
}