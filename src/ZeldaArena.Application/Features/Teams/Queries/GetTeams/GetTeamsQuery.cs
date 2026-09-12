using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Teams.Queries.GetTeams;

public sealed record GetTeamsQuery : FilterBase, IQuery<PagedResult<TeamListItemDto>>
{
    public Region? Region { get; init; }

    /// <summary>Рейтинг не ниже указанного.</summary>
    public int? RatingMin { get; init; }

    /// <summary>Поиск по названию и тегу.</summary>
    public string? Search { get; init; }
}