using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Security;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams.Queries.GetMyTeam;

public sealed class GetMyTeamQueryHandler(
    IReadRepository<Team> teams,
    IReadRepository<RosterEntry> rosterEntries,
    IReadRepository<Player> players,
    IEntitlementService entitlements,
    ICurrentUserService currentUser,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetMyTeamQuery, MyTeamDto?>
{
    public async Task<MyTeamDto?> Handle(GetMyTeamQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (currentUser.UserId is not { } ownerId)
        {
            return null;
        }

        var owned = await queryExecutor.ToListAsync(
            teams.Query()
                .Where(team => team.OwnerUserId == ownerId)
                .OrderBy(team => team.CreatedAt)
                .Select(team => new OwnedTeamDto(team.Id, team.Name)),
            cancellationToken).ConfigureAwait(false);

        var selectedId = request.TeamId is { } requested
            ? owned.FirstOrDefault(team => team.Id == requested)?.Id
            : owned.FirstOrDefault()?.Id;

        if (selectedId is not { } teamId)
        {
            return null;
        }

        var team = await queryExecutor.FirstOrDefaultAsync(
            teams.Query()
                .Where(team => team.Id == teamId)
                .Select(team => new MyTeamDto
                {
                    Id = team.Id,
                    Slug = team.Slug.Value,
                    Name = team.Name,
                    Tag = team.Tag,
                    CountryCode = team.Country.Value,
                    Region = team.Region,
                    FoundedAt = team.FoundedAt,
                    Description = team.Description,
                    LogoPath = team.LogoPath,
                    IsApproved = team.IsApproved,
                    Rating = team.Rating,
                }),
            cancellationToken).ConfigureAwait(false);

        if (team is null)
        {
            return null;
        }

        var roster = await queryExecutor
            .ToListAsync(TeamRosterQuery.Active(rosterEntries, players, teamId), cancellationToken)
            .ConfigureAwait(false);

        var canEdit = await entitlements
            .CurrentUserHasAsync(currentUser, FeatureCodes.TeamCreate, cancellationToken)
            .ConfigureAwait(false);

        return team with { Roster = roster, CanEdit = canEdit, OwnedTeams = owned };
    }
}