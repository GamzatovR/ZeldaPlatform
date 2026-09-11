using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Teams.Queries.GetTeams;

/// <summary>
/// Список команд (docs/SPEC.md §9.3, п. 6): регион, рейтинг, поиск, пагинация.
/// Тот же механизм фильтрации, что у турниров (§10.2): поля повторяют query-string
/// <c>/teams?region=europe&amp;ratingMin=1500&amp;search=hyrule&amp;sort=rating_desc&amp;page=2</c>.
/// </summary>
public sealed record GetTeamsQuery : FilterBase, IQuery<PagedResult<TeamListItemDto>>
{
    public Region? Region { get; init; }

    /// <summary>Рейтинг не ниже указанного.</summary>
    public int? RatingMin { get; init; }

    /// <summary>Поиск по названию и тегу.</summary>
    public string? Search { get; init; }
}