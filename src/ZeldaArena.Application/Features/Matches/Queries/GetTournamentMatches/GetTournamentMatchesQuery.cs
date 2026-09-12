using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Esports;

namespace ZeldaArena.Application.Features.Matches.Queries.GetTournamentMatches;

public sealed record GetTournamentMatchesQuery : FilterBase, IQuery<PagedResult<MatchCardDto>>
{
    public Guid TournamentId { get; init; }

    public MatchListState State { get; init; }
}