using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Matches.Queries.GetTournamentMatches;

public sealed class GetTournamentMatchesQueryHandler(
    IReadRepository<Match> matches,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetTournamentMatchesQuery, PagedResult<MatchCardDto>>
{
    public Task<PagedResult<MatchCardDto>> Handle(GetTournamentMatchesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = matches.Query().Where(match => match.TournamentId == request.TournamentId);

        IOrderedQueryable<Match> ordered = request.State switch
        {
            MatchListState.Upcoming => query
                .Where(match => match.Status == MatchStatus.Live
                    || match.Status == MatchStatus.Scheduled
                    || match.Status == MatchStatus.Postponed)
                .OrderBy(match => match.Status == MatchStatus.Live ? 0 : 1)
                .ThenBy(match => match.ScheduledAt),

            MatchListState.Past => query
                .Where(match => match.Status == MatchStatus.Finished
                    || match.Status == MatchStatus.Canceled)
                .OrderByDescending(match => match.ScheduledAt),

            _ => query.OrderBy(match => match.ScheduledAt),
        };

        return queryExecutor.ToPagedResultAsync(
            ordered.ThenBy(match => match.Id).Select(MatchCardProjection.Expression),
            request.NormalizedPage,
            request.NormalizedPageSize,
            cancellationToken);
    }
}