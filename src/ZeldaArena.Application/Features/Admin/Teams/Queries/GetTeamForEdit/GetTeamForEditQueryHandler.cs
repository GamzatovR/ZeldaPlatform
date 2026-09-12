using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Features.Teams;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Teams.Queries.GetTeamForEdit;

/// <summary>
/// Состав берётся тем же <see cref="TeamRosterQuery"/>, что и на публичной странице
/// команды и в кабинете капитана: одно определение «кто сейчас в составе» на три места.
/// </summary>
public sealed class GetTeamForEditQueryHandler(
    IReadRepository<Team> teams,
    IReadRepository<RosterEntry> rosterEntries,
    IReadRepository<Player> players,
    IReadRepository<Match> matches,
    IReadRepository<Tournament> tournaments,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetTeamForEditQuery, TeamEditDto?>
{
    /// <summary>Столько свободных игроков показывает форма; поиск по ним — Фаза 8 сайта.</summary>
    public const int FreeAgentLimit = 100;

    public async Task<TeamEditDto?> Handle(GetTeamForEditQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var id = request.Id;

        var team = await queryExecutor.FirstOrDefaultAsync(
            teams.Query()
                .Where(item => item.Id == id)
                .Select(item => new TeamEditDto
                {
                    Id = item.Id,
                    Slug = item.Slug.Value,
                    Name = item.Name,
                    Tag = item.Tag,
                    CountryCode = item.Country.Value,
                    Region = item.Region,
                    FoundedAt = item.FoundedAt,
                    Description = item.Description,
                    LogoPath = item.LogoPath,
                    Rating = item.Rating,
                    IsApproved = item.IsApproved,
                    IsUserOwned = item.OwnerUserId != null,
                    MatchCount = matches.Query().Count(match => match.TeamAId == id || match.TeamBId == id),
                    TournamentCount = tournaments.Query()
                        .SelectMany(tournament => tournament.Participants)
                        .Count(participant => participant.TeamId == id),
                    RosterEntryCount = item.RosterEntries.Count(),
                }),
            cancellationToken);

        if (team is null)
        {
            return null;
        }

        var roster = await queryExecutor.ToListAsync(
            TeamRosterQuery.Active(rosterEntries, players, id),
            cancellationToken);

        var free = await queryExecutor.ToListAsync(
            players.Query()
                .Where(player => !rosterEntries.Query()
                    .Any(entry => entry.PlayerId == player.Id && entry.LeftAt == null))
                .OrderBy(player => player.Nickname)
                .Take(FreeAgentLimit)
                .Select(player => new FreeAgentDto(player.Id, player.Nickname, player.Role)),
            cancellationToken);

        return team with { Roster = roster, FreeAgents = free };
    }
}