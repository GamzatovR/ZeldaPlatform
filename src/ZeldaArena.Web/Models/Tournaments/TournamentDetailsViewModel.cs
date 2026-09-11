using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Features.Matches.Queries.GetTournamentMatches;
using ZeldaArena.Application.Features.Tournaments.Queries.GetTournamentBySlug;

namespace ZeldaArena.Web.Models.Tournaments;

/// <summary>
/// Страница турнира (docs/SPEC.md §9.3, п. 4): сам турнир и матчи выбранной вкладки.
/// </summary>
public sealed class TournamentDetailsViewModel
{
    public required TournamentDetailsDto Tournament { get; init; }

    public required MatchListState State { get; init; }

    public required PagedResult<MatchCardDto> Matches { get; init; }
}