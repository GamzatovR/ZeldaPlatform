using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Players.Queries.GetPlayerBySlug;

public sealed class GetPlayerBySlugQueryHandler(
    IReadRepository<Player> players,
    IReadRepository<RosterEntry> rosterEntries,
    IReadRepository<Team> teams,
    IReadRepository<PlayerMatchStats> stats,
    IReadRepository<Match> matches,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetPlayerBySlugQuery, PlayerDetailsDto?>
{
    public const int RecentCount = 5;

    public async Task<PlayerDetailsDto?> Handle(GetPlayerBySlugQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!Slug.TryFrom(request.Slug, out var slug) || slug is null)
        {
            return null;
        }

        var player = await queryExecutor.FirstOrDefaultAsync(
            players.Query()
                .Where(player => player.Slug == slug)
                .Select(player => new PlayerDetailsDto
                {
                    Id = player.Id,
                    Slug = player.Slug.Value,
                    Nickname = player.Nickname,
                    FirstName = player.FirstName,
                    LastName = player.LastName,
                    CountryCode = player.Country.Value,
                    BirthDate = player.BirthDate,
                    Role = player.Role,
                    AvatarPath = player.AvatarPath,
                    Bio = player.Bio,
                }),
            cancellationToken).ConfigureAwait(false);

        if (player is null)
        {
            return null;
        }

        var history = await queryExecutor.ToListAsync(
            from entry in rosterEntries.Query()
            join team in teams.Query() on entry.TeamId equals team.Id
            where entry.PlayerId == player.Id && team.IsApproved
            orderby entry.LeftAt == null descending, entry.JoinedAt descending
            select new PlayerTeamHistoryDto
            {
                TeamSlug = team.Slug.Value,
                TeamName = team.Name,
                Role = entry.Role,
                JoinedAt = entry.JoinedAt,
                LeftAt = entry.LeftAt,
            },
            cancellationToken).ConfigureAwait(false);

        // Одна группа по игроку — это агрегат в базе, а не выгрузка всех строк в память.
        var totals = await queryExecutor.FirstOrDefaultAsync(
            stats.Query()
                .Where(line => line.PlayerId == player.Id)
                .GroupBy(line => line.PlayerId)
                .Select(group => new
                {
                    Matches = group.Count(),
                    Kills = group.Average(line => (double)line.Kills),
                    Deaths = group.Average(line => (double)line.Deaths),
                    Assists = group.Average(line => (double)line.Assists),
                    Rating = group.Average(line => (double)line.Rating),
                }),
            cancellationToken).ConfigureAwait(false);

        var recent = await queryExecutor.ToListAsync(
            matches.Query()
                .Where(match => stats.Query().Any(line => line.MatchId == match.Id && line.PlayerId == player.Id))
                .OrderByDescending(match => match.ScheduledAt)
                .Take(RecentCount)
                .Select(MatchCardProjection.Expression),
            cancellationToken).ConfigureAwait(false);

        return player with
        {
            Teams = history,
            MatchesPlayed = totals?.Matches ?? 0,
            AverageKills = totals?.Kills ?? 0,
            AverageDeaths = totals?.Deaths ?? 0,
            AverageAssists = totals?.Assists ?? 0,
            AverageRating = totals?.Rating ?? 0,
            RecentMatches = recent,
        };
    }
}