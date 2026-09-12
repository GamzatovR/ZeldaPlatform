using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchesForAdmin;

public sealed class GetMatchesForAdminQueryHandler(
    IReadRepository<Match> matches,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetMatchesForAdminQuery, PagedResult<AdminMatchRowDto>>
{
    public Task<PagedResult<AdminMatchRowDto>> Handle(
        GetMatchesForAdminQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = matches.Query();

        if (request.TournamentId is { } tournamentId)
        {
            query = query.Where(match => match.TournamentId == tournamentId);
        }

        if (request.Status is { } status)
        {
            query = query.Where(match => match.Status == status);
        }

        var rows = AdminMatchSorting.Map
            .Apply(query, request.Sort)
            .Select(match => new AdminMatchRowDto
            {
                Id = match.Id,
                TournamentName = match.Tournament == null ? string.Empty : match.Tournament.Name,
                TeamAName = match.TeamA == null ? string.Empty : match.TeamA.Name,
                TeamBName = match.TeamB == null ? string.Empty : match.TeamB.Name,
                ScoreA = match.ScoreA,
                ScoreB = match.ScoreB,
                BestOf = match.BestOf,
                ScheduledAt = match.ScheduledAt,
                Status = match.Status,
            });

        return queryExecutor.ToPagedResultAsync(
            rows,
            request.NormalizedPage,
            request.NormalizedPageSize,
            cancellationToken);
    }
}