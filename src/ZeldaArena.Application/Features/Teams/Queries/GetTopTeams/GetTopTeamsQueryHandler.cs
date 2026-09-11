using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Features.Teams.Queries.GetTeams;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams.Queries.GetTopTeams;

/// <summary>Только одобренные команды — как в списке (docs/adr/ADR-0008).</summary>
public sealed class GetTopTeamsQueryHandler(IReadRepository<Team> teams, IQueryExecutor queryExecutor)
    : IRequestHandler<GetTopTeamsQuery, IReadOnlyList<TeamListItemDto>>
{
    public Task<IReadOnlyList<TeamListItemDto>> Handle(GetTopTeamsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return queryExecutor.ToListAsync(
            TeamSorting.Map
                .Apply(teams.Query().Where(team => team.IsApproved), TeamSorting.RatingDescending)
                .Take(request.Count)
                .Select(team => new TeamListItemDto
                {
                    Id = team.Id,
                    Slug = team.Slug.Value,
                    Name = team.Name,
                    Tag = team.Tag,
                    LogoPath = team.LogoPath,
                    CountryCode = team.Country.Value,
                    Region = team.Region,
                    Rating = team.Rating,
                    PlayerCount = team.RosterEntries.Count(entry => entry.LeftAt == null),
                }),
            cancellationToken);
    }
}