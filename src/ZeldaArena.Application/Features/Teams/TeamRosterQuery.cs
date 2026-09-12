using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams;

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