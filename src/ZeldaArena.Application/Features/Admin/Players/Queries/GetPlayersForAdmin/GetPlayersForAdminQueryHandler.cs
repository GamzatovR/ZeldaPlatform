using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Players.Queries.GetPlayersForAdmin;

public sealed class GetPlayersForAdminQueryHandler(
    IReadRepository<Player> players,
    IReadRepository<RosterEntry> rosterEntries,
    IReadRepository<Team> teams,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetPlayersForAdminQuery, PagedResult<AdminPlayerRowDto>>
{
    public Task<PagedResult<AdminPlayerRowDto>> Handle(
        GetPlayersForAdminQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = players.Query();

        if (request.Role is { } role)
        {
            query = query.Where(player => player.Role == role);
        }

        if (request.IsFreeAgent is { } free)
        {
            query = free
                ? query.Where(player => !rosterEntries.Query().Any(entry => entry.PlayerId == player.Id && entry.LeftAt == null))
                : query.Where(player => rosterEntries.Query().Any(entry => entry.PlayerId == player.Id && entry.LeftAt == null));
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = request.Search.Trim().ToLowerInvariant();

            query = query.Where(player => player.Nickname.ToLower().Contains(pattern));
        }

        var rows = AdminPlayerSorting.Map
            .Apply(query, request.Sort)
            .Select(player => new AdminPlayerRowDto
            {
                Id = player.Id,
                Slug = player.Slug.Value,
                Nickname = player.Nickname,
                FullName = player.FirstName == null && player.LastName == null
                    ? null
                    : (player.FirstName ?? string.Empty) + " " + (player.LastName ?? string.Empty),
                CountryCode = player.Country.Value,
                Role = player.Role,
                TeamName = (from entry in rosterEntries.Query()
                            join team in teams.Query() on entry.TeamId equals team.Id
                            where entry.PlayerId == player.Id && entry.LeftAt == null
                            select team.Name).FirstOrDefault(),
            });

        return queryExecutor.ToPagedResultAsync(
            rows,
            request.NormalizedPage,
            request.NormalizedPageSize,
            cancellationToken);
    }
}