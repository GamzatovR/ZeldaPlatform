using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Teams.Queries.GetTeamsForAdmin;

public sealed class GetTeamsForAdminQueryHandler(
    IReadRepository<Team> teams,
    IReadRepository<Match> matches,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetTeamsForAdminQuery, PagedResult<AdminTeamRowDto>>
{
    public Task<PagedResult<AdminTeamRowDto>> Handle(
        GetTeamsForAdminQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = teams.Query();

        if (request.IsApproved is { } isApproved)
        {
            query = query.Where(team => team.IsApproved == isApproved);
        }

        if (request.Region is { } region)
        {
            query = query.Where(team => team.Region == region);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = request.Search.Trim().ToLowerInvariant();

            query = query.Where(team => team.Name.ToLower().Contains(pattern)
                || team.Tag.ToLower().Contains(pattern));
        }

        var rows = AdminTeamSorting.Map
            .Apply(query, request.Sort)
            .Select(team => new AdminTeamRowDto
            {
                Id = team.Id,
                Slug = team.Slug.Value,
                Name = team.Name,
                Tag = team.Tag,
                Region = team.Region,
                CountryCode = team.Country.Value,
                Rating = team.Rating,
                IsApproved = team.IsApproved,
                IsUserOwned = team.OwnerUserId != null,
                RosterCount = team.RosterEntries.Count(entry => entry.LeftAt == null),
                MatchCount = matches.Query().Count(match => match.TeamAId == team.Id || match.TeamBId == team.Id),
            });

        return queryExecutor.ToPagedResultAsync(
            rows,
            request.NormalizedPage,
            request.NormalizedPageSize,
            cancellationToken);
    }
}