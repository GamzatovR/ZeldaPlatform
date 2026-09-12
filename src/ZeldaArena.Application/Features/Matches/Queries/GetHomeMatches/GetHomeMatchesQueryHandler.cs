using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Matches.Queries.GetHomeMatches;

public sealed class GetHomeMatchesQueryHandler(
    IReadRepository<Match> matches,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetHomeMatchesQuery, IReadOnlyList<MatchCardDto>>
{
    public Task<IReadOnlyList<MatchCardDto>> Handle(
        GetHomeMatchesQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = matches
            .Query()
            .Where(match => match.Status == MatchStatus.Live
                || match.Status == MatchStatus.Scheduled)
            .OrderBy(match => match.Status == MatchStatus.Live ? 0 : 1)
            .ThenBy(match => match.ScheduledAt)
            .Take(request.Count)
            .Select(MatchCardProjection.Expression);

        return queryExecutor.ToListAsync(query, cancellationToken);
    }
}