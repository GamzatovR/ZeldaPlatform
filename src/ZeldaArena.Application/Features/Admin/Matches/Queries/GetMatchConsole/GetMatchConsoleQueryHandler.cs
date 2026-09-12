using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Admin.Matches.Queries.GetMatchConsole;

public sealed class GetMatchConsoleQueryHandler(
    IReadRepository<Match> matches,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetMatchConsoleQuery, MatchConsoleDto?>
{
    public Task<MatchConsoleDto?> Handle(GetMatchConsoleQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = matches.Query()
            .Where(match => match.Id == request.Id)
            .Select(match => new MatchConsoleDto
            {
                Id = match.Id,
                TournamentId = match.TournamentId,
                TournamentName = match.Tournament == null ? string.Empty : match.Tournament.Name,
                TournamentSlug = match.Tournament == null ? string.Empty : match.Tournament.Slug.Value,
                TeamAName = match.TeamA == null ? string.Empty : match.TeamA.Name,
                TeamBName = match.TeamB == null ? string.Empty : match.TeamB.Name,
                ScoreA = match.ScoreA,
                ScoreB = match.ScoreB,
                BestOf = match.BestOf,
                ScheduledAt = match.ScheduledAt,
                Status = match.Status,
                StreamUrl = match.StreamUrl,
                PlayerStatsCount = match.PlayerStats.Count(),
            });

        return queryExecutor.FirstOrDefaultAsync(query, cancellationToken);
    }
}