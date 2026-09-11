using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Esports;

namespace ZeldaArena.Application.Features.Matches.Queries.GetTournamentMatches;

/// <summary>
/// Матчи турнира во вкладке с пагинацией. Тот же запрос отдаёт эндпоинт
/// <c>GET /api/tournaments/{slug}/matches?state=upcoming</c> (docs/SPEC.md §10.1, сценарий 2) —
/// поэтому это отдельный сценарий, а не часть страницы турнира.
/// </summary>
public sealed record GetTournamentMatchesQuery : FilterBase, IQuery<PagedResult<MatchCardDto>>
{
    public Guid TournamentId { get; init; }

    public MatchListState State { get; init; }
}