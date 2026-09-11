using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Teams.Queries.GetTeams;

/// <summary>
/// Только одобренные команды: команда подписчика до одобрения модератором в списке
/// не показывается (решение Фазы 6, docs/adr/ADR-0008). Владелец видит её в своём
/// кабинете и по прямой ссылке.
///
/// Размер состава — <c>Count()</c> с условием внутри проекции: EF Core переводит его
/// в скалярный подзапрос, а не тянет записи состава каждой команды (§16, урок Фазы 2).
/// </summary>
public sealed class GetTeamsQueryHandler(
    IReadRepository<Team> teams,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetTeamsQuery, PagedResult<TeamListItemDto>>
{
    public Task<PagedResult<TeamListItemDto>> Handle(GetTeamsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = teams.Query().Where(team => team.IsApproved);

        if (request.Region is { } region)
        {
            query = query.Where(team => team.Region == region);
        }

        if (request.RatingMin is { } ratingMin)
        {
            query = query.Where(team => team.Rating >= ratingMin);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = request.Search.Trim().ToLowerInvariant();

            query = query.Where(team => team.Name.ToLower().Contains(pattern)
                || team.Tag.ToLower().Contains(pattern));
        }

        var projected = TeamSorting.Map
            .Apply(query, request.Sort)
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
            });

        return queryExecutor.ToPagedResultAsync(
            projected,
            request.NormalizedPage,
            request.NormalizedPageSize,
            cancellationToken);
    }
}