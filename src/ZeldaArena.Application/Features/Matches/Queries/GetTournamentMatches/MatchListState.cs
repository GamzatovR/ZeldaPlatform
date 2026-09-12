namespace ZeldaArena.Application.Features.Matches.Queries.GetTournamentMatches;

public enum MatchListState
{
    All = 0,

    /// <summary>Ещё не сыгранные: идущие, запланированные и перенесённые.</summary>
    Upcoming = 1,

    /// <summary>Сыгранные и отменённые.</summary>
    Past = 2,
}