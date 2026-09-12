using ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchConsole;

namespace ZeldaArena.Web.Areas.Admin.Models.Matches;

/// <summary>
/// Состояние пульта для клиента: то, что admin-console.js раскладывает по разметке.
///
/// Один тип на первую отрисовку страницы и на ответы API — иначе два представления
/// разошлись бы (например, статус числом в одном месте и строкой в другом), и пульт
/// после каждой правки счёта считал бы статус изменившимся.
/// </summary>
public sealed record MatchConsoleState(int ScoreA, int ScoreB, bool CanFinish, string Status)
{
    public static MatchConsoleState From(MatchConsoleDto match)
    {
        ArgumentNullException.ThrowIfNull(match);

        return new MatchConsoleState(match.ScoreA, match.ScoreB, match.CanFinish, match.Status.ToString());
    }
}