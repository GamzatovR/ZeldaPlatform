using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Players.Queries.GetPlayers;

public sealed record GetPlayersQuery : FilterBase, IQuery<PagedResult<PlayerListItemDto>>
{
    public PlayerRole? Role { get; init; }

    /// <summary>Код страны ISO 3166-1 alpha-2.</summary>
    public string? Country { get; init; }

    /// <summary>Слаг текущей команды.</summary>
    public string? Team { get; init; }

    /// <summary>Поиск по нику, имени и фамилии.</summary>
    public string? Search { get; init; }
}