using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Players.Queries.GetPlayerForEdit;

public sealed class GetPlayerForEditQueryHandler(
    IReadRepository<Player> players,
    IReadRepository<RosterEntry> rosterEntries,
    IReadRepository<Team> teams,
    IReadRepository<Match> matches,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetPlayerForEditQuery, PlayerEditDto?>
{
    public Task<PlayerEditDto?> Handle(GetPlayerForEditQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var id = request.Id;

        var query = players.Query()
            .Where(player => player.Id == id)
            .Select(player => new PlayerEditDto
            {
                Id = player.Id,
                Slug = player.Slug.Value,
                Nickname = player.Nickname,
                CountryCode = player.Country.Value,
                Role = player.Role,
                FirstName = player.FirstName,
                LastName = player.LastName,
                BirthDate = player.BirthDate,
                Bio = player.Bio,
                AvatarPath = player.AvatarPath,
                TeamName = (from entry in rosterEntries.Query()
                            join team in teams.Query() on entry.TeamId equals team.Id
                            where entry.PlayerId == id && entry.LeftAt == null
                            select team.Name).FirstOrDefault(),
                RosterEntryCount = rosterEntries.Query().Count(entry => entry.PlayerId == id),
                StatsCount = matches.Query().SelectMany(match => match.PlayerStats).Count(stats => stats.PlayerId == id),
            });

        return queryExecutor.FirstOrDefaultAsync(query, cancellationToken);
    }
}