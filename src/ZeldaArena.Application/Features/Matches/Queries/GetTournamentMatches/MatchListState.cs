namespace ZeldaArena.Application.Features.Matches.Queries.GetTournamentMatches;

/// <summary>
/// Вкладки матчей на странице турнира (docs/SPEC.md §9.3, п. 4): Все / Предстоящие / Прошедшие.
/// Значение приходит из адреса (<c>?state=upcoming</c>), поэтому вкладка переживает F5
/// и открывается по пересланной ссылке (§10.2).
/// </summary>
public enum MatchListState
{
    All = 0,

    /// <summary>Ещё не сыгранные: идущие, запланированные и перенесённые.</summary>
    Upcoming = 1,

    /// <summary>Сыгранные и отменённые.</summary>
    Past = 2,
}