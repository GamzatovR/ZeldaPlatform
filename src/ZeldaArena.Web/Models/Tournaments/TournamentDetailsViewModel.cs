using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Features.Matches.Queries.GetTournamentMatches;
using ZeldaArena.Application.Features.Tournaments.Queries.GetTournamentBySlug;

namespace ZeldaArena.Web.Models.Tournaments;

public sealed class TournamentDetailsViewModel
{
    public required TournamentDetailsDto Tournament { get; init; }

    public required MatchListState State { get; init; }

    public required PagedResult<MatchCardDto> Matches { get; init; }
}