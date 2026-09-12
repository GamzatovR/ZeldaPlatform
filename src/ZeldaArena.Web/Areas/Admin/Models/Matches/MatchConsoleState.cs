using ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchConsole;

namespace ZeldaArena.Web.Areas.Admin.Models.Matches;

public sealed record MatchConsoleState(int ScoreA, int ScoreB, bool CanFinish, string Status)
{
    public static MatchConsoleState From(MatchConsoleDto match)
    {
        ArgumentNullException.ThrowIfNull(match);

        return new MatchConsoleState(match.ScoreA, match.ScoreB, match.CanFinish, match.Status.ToString());
    }
}