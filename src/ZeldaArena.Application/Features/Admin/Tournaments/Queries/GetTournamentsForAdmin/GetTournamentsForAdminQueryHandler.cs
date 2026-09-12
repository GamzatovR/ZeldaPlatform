using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Tournaments.Queries.GetTournamentsForAdmin;

/// <summary>
/// Турниры в любом статусе, включая отменённые. Число участников и матчей считается
/// в SQL скалярным подзапросом (<c>Count()</c> методом, а не свойством — урок Фазы 2:
/// свойство <c>Count</c> EF поднимал бы в память всю коллекцию ради одного числа).
/// </summary>
public sealed class GetTournamentsForAdminQueryHandler(
    IReadRepository<Tournament> tournaments,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetTournamentsForAdminQuery, PagedResult<AdminTournamentRowDto>>
{
    public Task<PagedResult<AdminTournamentRowDto>> Handle(
        GetTournamentsForAdminQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = tournaments.Query();

        if (request.Status is { } status)
        {
            query = query.Where(tournament => tournament.Status == status);
        }

        if (request.Region is { } region)
        {
            query = query.Where(tournament => tournament.Region == region);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = request.Search.Trim().ToLowerInvariant();

            query = query.Where(tournament => tournament.Name.ToLower().Contains(pattern));
        }

        var rows = AdminTournamentSorting.Map
            .Apply(query, request.Sort)
            .Select(tournament => new AdminTournamentRowDto
            {
                Id = tournament.Id,
                Slug = tournament.Slug.Value,
                Name = tournament.Name,
                Tier = tournament.Tier,
                Region = tournament.Region,
                Status = tournament.Status,
                StartsAt = tournament.StartsAt,
                EndsAt = tournament.EndsAt,
                PrizePool = tournament.PrizePool.Amount,
                Currency = tournament.PrizePool.Currency,
                IsFeatured = tournament.IsFeatured,
                ParticipantCount = tournament.Participants.Count(),
                MatchCount = tournament.Matches.Count(),
            });

        return queryExecutor.ToPagedResultAsync(
            rows,
            request.NormalizedPage,
            request.NormalizedPageSize,
            cancellationToken);
    }
}