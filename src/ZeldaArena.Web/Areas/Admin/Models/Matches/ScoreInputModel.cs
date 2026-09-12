using System.ComponentModel.DataAnnotations;

using ZeldaArena.Application.Features.Matches.Commands.UpdateMatchScore;

namespace ZeldaArena.Web.Areas.Admin.Models.Matches;

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