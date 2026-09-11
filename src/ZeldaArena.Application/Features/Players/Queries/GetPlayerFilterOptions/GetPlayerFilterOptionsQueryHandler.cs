using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Players.Queries.GetPlayerFilterOptions;

public sealed class GetPlayerFilterOptionsQueryHandler(
    IReadRepository<Player> players,
    IReadRepository<Team> teams,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetPlayerFilterOptionsQuery, PlayerFilterOptionsDto>
{
    public async Task<PlayerFilterOptionsDto> Handle(
        GetPlayerFilterOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var countries = await queryExecutor.ToListAsync(
            players.Query().Select(player => player.Country).Distinct(),
            cancellationToken).ConfigureAwait(false);

        var teamOptions = await queryExecutor.ToListAsync(
            teams.Query()
                .Where(team => team.IsApproved)
                .OrderBy(team => team.Name)
                .Select(team => new TeamOptionDto(team.Slug.Value, team.Name)),
            cancellationToken).ConfigureAwait(false);

        return new PlayerFilterOptionsDto
        {
            Countries = [.. countries.Select(country => country.Value).Order(StringComparer.Ordinal)],
            Teams = teamOptions,
        };
    }
}