using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Players.Queries.GetPlayers;

/// <summary>Разрешённые сортировки списка игроков — whitelist из docs/SPEC.md §15.</summary>
public static class PlayerSorting
{
    public const string NicknameAscending = "nickname_asc";
    public const string NicknameDescending = "nickname_desc";

    public static readonly SortMap<Player> Map = new SortMap<Player>()
        .Add(NicknameAscending, query => query.OrderBy(player => player.Nickname), isDefault: true)
        .Add(NicknameDescending, query => query.OrderByDescending(player => player.Nickname));
}