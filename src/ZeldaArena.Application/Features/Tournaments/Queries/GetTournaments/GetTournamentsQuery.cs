using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Tournaments.Queries.GetTournaments;

public sealed record GetTournamentsQuery
    : FilterBase, IQuery<PagedResult<TournamentListItemDto>>
{
    public TournamentStatus? Status { get; init; }

    public Region? Region { get; init; }

    /// <summary>Турниры, начинающиеся не раньше указанной даты.</summary>
    public DateOnly? From { get; init; }

    /// <summary>Турниры, начинающиеся не позже указанной даты (включительно).</summary>
    public DateOnly? To { get; init; }

    /// <summary>Призовой фонд не меньше указанного.</summary>
    public decimal? PrizeMin { get; init; }

    public string? Search { get; init; }
}