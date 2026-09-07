using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Tournaments.Queries.GetTournaments;

/// <summary>
/// Список турниров с фильтром, сортировкой и пагинацией. Набор полей повторяет
/// query-string из docs/SPEC.md §10.2:
/// <c>/tournaments?status=ongoing&amp;region=eu&amp;from=2025-01-01&amp;sort=prize_desc&amp;page=2</c>.
///
/// Источник истины о состоянии списка — URL, поэтому у запроса нет ни одного поля,
/// которое нельзя было бы выразить ссылкой (§10.2).
/// </summary>
public sealed record GetTournamentsQuery
    : FilterBase, IQuery<PagedResult<TournamentListItemDto>>
{
    public TournamentStatus? Status { get; init; }

    public Region? Region { get; init; }

    /// <summary>Турниры, начинающиеся не раньше указанной даты.</summary>
    public DateOnly? From { get; init; }

    public string? Search { get; init; }
}