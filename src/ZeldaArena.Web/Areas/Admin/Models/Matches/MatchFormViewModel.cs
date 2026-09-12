using System.ComponentModel.DataAnnotations;

using ZeldaArena.Application.Features.Admin.Matches.Commands.ScheduleMatch;

namespace ZeldaArena.Web.Areas.Admin.Models.Matches;

/// <summary>
/// Назначение матча. Пределы те же, что у <see cref="ScheduleMatchCommandValidator"/>;
/// нечётность формата серии проверяет сущность, поэтому в списке только Bo1/Bo3/Bo5/Bo7.
/// </summary>
public sealed class MatchFormViewModel : IValidatableObject
{
    public static readonly int[] SeriesFormats = [1, 3, 5, 7];

    [Required(ErrorMessage = "Выберите турнир.")]
    [Display(Name = "Турнир")]
    public Guid TournamentId { get; set; }

    [Required(ErrorMessage = "Выберите первую команду.")]
    [Display(Name = "Первая команда")]
    public Guid TeamAId { get; set; }

    [Required(ErrorMessage = "Выберите вторую команду.")]
    [Display(Name = "Вторая команда")]
    public Guid TeamBId { get; set; }

    [Required(ErrorMessage = "Укажите время начала.")]
    [Display(Name = "Начало (UTC)")]
    public DateTime ScheduledAt { get; set; }

    [Range(1, ScheduleMatchCommandValidator.MaxBestOf, ErrorMessage = "Формат серии — от Bo{1} до Bo{2}.")]
    [Display(Name = "Формат серии")]
    public int BestOf { get; set; } = 3;

    [StringLength(ScheduleMatchCommandValidator.MaxStreamUrlLength, ErrorMessage = "Ссылка не длиннее {1} символов.")]
    [Display(Name = "Ссылка на трансляцию")]
    public string? StreamUrl { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (TeamAId != Guid.Empty && TeamAId == TeamBId)
        {
            yield return new ValidationResult("Команда не может играть сама с собой.", [nameof(TeamBId)]);
        }

        if (!SeriesFormats.Contains(BestOf))
        {
            yield return new ValidationResult("Формат серии — Bo1, Bo3, Bo5 или Bo7.", [nameof(BestOf)]);
        }
    }
}