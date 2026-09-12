using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Players.Queries.GetPlayersForAdmin;

/// <summary>Таблица игроков — <c>/admin/players</c> (docs/SPEC.md §9.4, п. 5).</summary>
public sealed record GetPlayersForAdminQuery : FilterBase, IQuery<PagedResult<AdminPlayerRowDto>>
{
    public string? Search { get; init; }

    public PlayerRole? Role { get; init; }

    /// <summary><see langword="true"/> — только свободные игроки, без действующей команды.</summary>
    public bool? IsFreeAgent { get; init; }
}