using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Matches.Queries.GetHomeMatches;

/// <summary>
/// Идущие матчи впереди ближайших запланированных, внутри каждой группы — по времени.
/// Порядок задаётся выражением, а не двумя запросами: одна выборка вместо двух,
/// и никакой склейки в памяти.
///
/// Проекция строится сразу в DTO, поэтому составы команд, статистика игроков
/// и записи турнира из базы не поднимаются. Обязательная проверка на N+1 из §16:
/// названия и логотипы команд читаются внутри проекции, и EF Core разворачивает
/// их в JOIN, а не в отдельный запрос на строку — ровно та ошибка, что была
/// поймана на списке турниров в Фазе 2.
///
/// Проекция — общая <see cref="MatchCardProjection"/>: та же карточка стоит
/// на странице турнира, в расписании и в истории команды.
/// </summary>
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