using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Features.Tournaments.Queries.GetTournaments;

namespace ZeldaArena.Web.Models.Tournaments;

public sealed class TournamentListViewModel
{
    public required GetTournamentsQuery Filter { get; init; }

    public required PagedResult<TournamentListItemDto> Result { get; init; }
}