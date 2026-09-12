using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentsForAdmin;

/// <summary>
/// Таблица турниров админки — <c>/admin/tournaments</c> (docs/SPEC.md §9.4, п. 2).
/// Тот же механизм, что у публичных списков (§10.2): фильтр, whitelist-сортировка
/// и страница приходят из адреса.
/// </summary>
public sealed record GetTournamentsForAdminQuery : FilterBase, IQuery<PagedResult<AdminTournamentRowDto>>
{
    public string? Search { get; init; }

    public TournamentStatus? Status { get; init; }

    public Region? Region { get; init; }
}