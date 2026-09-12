using Mapster;

using MapsterMapper;

using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Tournaments.Queries.GetTournaments;

public sealed class GetTournamentsQueryHandler(
    IReadRepository<Tournament> tournaments,
    IQueryExecutor queryExecutor,
    IMapper mapper)
    : IRequestHandler<GetTournamentsQuery, PagedResult<TournamentListItemDto>>
{
    public Task<PagedResult<TournamentListItemDto>> Handle(
        GetTournamentsQuery request,
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

        if (request.From is { } from)
        {
            var startOfDay = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

            query = query.Where(tournament => tournament.StartsAt >= startOfDay);
        }

        if (request.To is { } to)
        {
            // «По дату включительно» — это «раньше начала следующего дня».
            var nextDay = new DateTimeOffset(to.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

            query = query.Where(tournament => tournament.StartsAt < nextDay);
        }

        if (request.PrizeMin is { } prizeMin)
        {
            query = query.Where(tournament => tournament.PrizePool.Amount >= prizeMin);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = request.Search.Trim().ToLowerInvariant();

            query = query.Where(tournament => tournament.Name.ToLower().Contains(pattern));
        }

        var projected = TournamentSorting.Map
            .Apply(query, request.Sort)
            .ProjectToType<TournamentListItemDto>(mapper.Config);

        return queryExecutor.ToPagedResultAsync(
            projected,
            request.NormalizedPage,
            request.NormalizedPageSize,
            cancellationToken);
    }
}