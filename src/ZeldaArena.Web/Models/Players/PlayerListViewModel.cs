using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Features.Players.Queries.GetPlayerFilterOptions;
using ZeldaArena.Application.Features.Players.Queries.GetPlayers;

namespace ZeldaArena.Web.Models.Players;

/// <summary>Список игроков (docs/SPEC.md §9.3, п. 9): фильтр из адреса, значения фильтра и страница результата.</summary>
public sealed class PlayerListViewModel
{
    public required GetPlayersQuery Filter { get; init; }

    public required PlayerFilterOptionsDto Options { get; init; }

    public required PagedResult<PlayerListItemDto> Result { get; init; }
}