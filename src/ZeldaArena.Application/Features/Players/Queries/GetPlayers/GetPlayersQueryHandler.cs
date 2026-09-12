using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Players.Queries.GetPlayers;

public sealed class GetPlayersQueryHandler(
    IReadRepository<Player> players,
    IReadRepository<RosterEntry> rosterEntries,
    IReadRepository<Team> teams,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetPlayersQuery, PagedResult<PlayerListItemDto>>
{
    public async Task<PagedResult<PlayerListItemDto>> Handle(GetPlayersQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = players.Query();
        var roster = rosterEntries.Query();
        var approvedTeams = teams.Query().Where(team => team.IsApproved);

        if (request.Role is { } role)
        {
            query = query.Where(player => player.Role == role);
        }

        if (!string.IsNullOrWhiteSpace(request.Country))
        {
            // Неизвестный код — пустой список, а не молча снятый фильтр.
            CountryCode.TryFrom(request.Country, out var country);

            query = query.Where(player => player.Country == country);
        }

        if (!string.IsNullOrWhiteSpace(request.Team))
        {
            Slug.TryFrom(request.Team, out var teamSlug);

            query = query.Where(player => roster.Any(entry =>
                entry.PlayerId == player.Id
                && entry.LeftAt == null
                && approvedTeams.Any(team => team.Id == entry.TeamId && team.Slug == teamSlug)));
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = request.Search.Trim().ToLowerInvariant();

            query = query.Where(player => player.Nickname.ToLower().Contains(pattern)
                || (player.FirstName != null && player.FirstName.ToLower().Contains(pattern))
                || (player.LastName != null && player.LastName.ToLower().Contains(pattern)));
        }

        var projected = PlayerSorting.Map
            .Apply(query, request.Sort)
            .Select(player => new PlayerListItemDto
            {
                Id = player.Id,
                Slug = player.Slug.Value,
                Nickname = player.Nickname,
                FirstName = player.FirstName,
                LastName = player.LastName,
                CountryCode = player.Country.Value,
                Role = player.Role,
                AvatarPath = player.AvatarPath,
                TeamId = roster
                    .Where(entry => entry.PlayerId == player.Id
                        && entry.LeftAt == null
                        && approvedTeams.Any(team => team.Id == entry.TeamId))
                    .Select(entry => (Guid?)entry.TeamId)
                    .FirstOrDefault(),
            });

        var page = await queryExecutor.ToPagedResultAsync(
            projected,
            request.NormalizedPage,
            request.NormalizedPageSize,
            cancellationToken).ConfigureAwait(false);

        // Названия команд — вторым запросом по идентификаторам страницы: слаг хранится
        // через конвертер, и его строковое значение внутри подзапроса SQL не выразить.
        var teamIds = page.Items.Where(item => item.TeamId != null).Select(item => item.TeamId!.Value).Distinct().ToList();
        var names = await queryExecutor.ToListAsync(
            teams.Query()
                .Where(team => teamIds.Contains(team.Id))
                .Select(team => new { team.Id, Slug = team.Slug.Value, team.Name }),
            cancellationToken).ConfigureAwait(false);

        var byId = names.ToDictionary(team => team.Id);

        return new PagedResult<PlayerListItemDto>(
            [
                .. page.Items.Select(item => item.TeamId is { } id && byId.TryGetValue(id, out var team)
                    ? item with { TeamName = team.Name, TeamSlug = team.Slug }
                    : item with { TeamId = null }),
            ],
            page.Page,
            page.PageSize,
            page.TotalCount);
    }
}