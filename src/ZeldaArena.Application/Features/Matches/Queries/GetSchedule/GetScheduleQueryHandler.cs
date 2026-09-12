using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Matches.Queries.GetSchedule;

public sealed class GetScheduleQueryHandler(
    IReadRepository<Match> matches,
    IReadRepository<Tournament> tournaments,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetScheduleQuery, ScheduleDto>
{
    public async Task<ScheduleDto> Handle(GetScheduleQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = matches.Query();

        if (!string.IsNullOrWhiteSpace(request.Tournament))
        {
            // Неразборчивый слаг — «такого турнира нет», и расписание честно пустое,
            // а не «фильтр молча сброшен на все турниры».
            Slug.TryFrom(request.Tournament, out var slug);

            query = query.Where(match => tournaments.Query()
                .Any(tournament => tournament.Id == match.TournamentId && tournament.Slug == slug));
        }

        var ordered = request.Status switch
        {
            null => query
                .Where(match => match.Status == MatchStatus.Live
                    || match.Status == MatchStatus.Scheduled
                    || match.Status == MatchStatus.Postponed)
                .OrderBy(match => match.ScheduledAt),

            MatchStatus.Finished or MatchStatus.Canceled => query
                .Where(match => match.Status == request.Status)
                .OrderByDescending(match => match.ScheduledAt),

            var status => query
                .Where(match => match.Status == status)
                .OrderBy(match => match.ScheduledAt),
        };

        var page = await queryExecutor.ToPagedResultAsync(
            ordered.ThenBy(match => match.Id).Select(MatchCardProjection.Expression),
            request.NormalizedPage,
            request.NormalizedPageSize,
            cancellationToken).ConfigureAwait(false);

        return new ScheduleDto
        {
            Days =
            [
                .. page.Items
                    .GroupBy(match => DateOnly.FromDateTime(match.ScheduledAt.UtcDateTime))
                    .Select(day => new ScheduleDayDto(day.Key, [.. day])),
            ],
            Page = page.Page,
            TotalPages = page.TotalPages,
            TotalCount = page.TotalCount,
        };
    }
}