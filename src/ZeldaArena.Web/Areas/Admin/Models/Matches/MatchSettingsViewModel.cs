using System.ComponentModel.DataAnnotations;

using ZeldaArena.Application.Features.Admin.Matches.Commands.ScheduleMatch;

namespace ZeldaArena.Web.Areas.Admin.Models.Matches;

/// <summary>Время и трансляция матча — то, что правится после назначения.</summary>
public sealed class MatchSettingsViewModel
{
    [Required(ErrorMessage = "Укажите время начала.")]
    [Display(Name = "Начало (UTC)")]
    public DateTime ScheduledAt { get; set; }

    [StringLength(ScheduleMatchCommandValidator.MaxStreamUrlLength, ErrorMessage = "Ссылка не длиннее {1} символов.")]
    [Display(Name = "Ссылка на трансляцию")]
    public string? StreamUrl { get; set; }
}