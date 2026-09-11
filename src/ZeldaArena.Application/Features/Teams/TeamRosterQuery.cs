using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams;

/// <summary>
/// Текущий состав команды — одна выборка на страницу команды и на кабинет капитана.
/// Игроки подтягиваются явным соединением, а не навигацией: тот же запрос исполняется
/// и в памяти, так что unit-тесты видят настоящие ники, а не пустые навигации.
/// </summary>
public static class TeamRosterQuery
{
    public static IQueryable<RosterPlayerDto> Active(
        IReadRepository<RosterEntry> rosterEntries,
        IReadRepository<Player> players,
        Guid teamId)
    {
        ArgumentNullException.ThrowIfNull(rosterEntries);
        ArgumentNullException.ThrowIfNull(players);

        return from entry in rosterEntries.Query()
               join player in players.Query() on entry.PlayerId equals player.Id
               where entry.TeamId == teamId && entry.LeftAt == null
               orderby entry.Role, player.Nickname
               select new RosterPlayerDto
               {
                   PlayerId = player.Id,
                   Slug = player.Slug.Value,
                   Nickname = player.Nickname,
                   Role = entry.Role,
                   CountryCode = player.Country.Value,
                   AvatarPath = player.AvatarPath,
                   JoinedAt = entry.JoinedAt,
               };
    }
}