using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Players.Queries.GetPlayersForAdmin;

public static class AdminPlayerSorting
{
    public const string NicknameAscending = "nickname_asc";
    public const string NicknameDescending = "nickname_desc";

    public static readonly SortMap<Player> Map = new SortMap<Player>()
        .Add(NicknameAscending, query => query.OrderBy(player => player.Nickname), isDefault: true)
        .Add(NicknameDescending, query => query.OrderByDescending(player => player.Nickname));
}