using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchesForAdmin;

/// <summary>
/// Таблица матчей — <c>/admin/matches</c> (docs/SPEC.md §9.4, п. 3). Фильтр по турниру
/// и статусу: пульт нужен модератору в день матча, и найти идущие он должен сразу.
/// </summary>
public sealed record GetMatchesForAdminQuery : FilterBase, IQuery<PagedResult<AdminMatchRowDto>>
{
    public Guid? TournamentId { get; init; }

    public MatchStatus? Status { get; init; }
}