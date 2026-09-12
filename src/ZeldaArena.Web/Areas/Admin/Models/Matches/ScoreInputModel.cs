using System.ComponentModel.DataAnnotations;

using ZeldaArena.Application.Features.Matches.Commands.UpdateMatchScore;

namespace ZeldaArena.Web.Areas.Admin.Models.Matches;

/// <summary>
/// Счёт из пульта вместе с тем, что модератор видел на экране: сервер сверяет
/// ожидаемый счёт с текущим и отвечает конфликтом, если его успели изменить (§15).
/// </summary>
public sealed class ScoreInputModel
{
    [Range(0, UpdateMatchScoreCommandValidator.MaxScore, ErrorMessage = "Счёт — от {1} до {2}.")]
    public int ScoreA { get; set; }

    [Range(0, UpdateMatchScoreCommandValidator.MaxScore, ErrorMessage = "Счёт — от {1} до {2}.")]
    public int ScoreB { get; set; }

    [Range(0, UpdateMatchScoreCommandValidator.MaxScore)]
    public int ExpectedScoreA { get; set; }

    [Range(0, UpdateMatchScoreCommandValidator.MaxScore)]
    public int ExpectedScoreB { get; set; }
}