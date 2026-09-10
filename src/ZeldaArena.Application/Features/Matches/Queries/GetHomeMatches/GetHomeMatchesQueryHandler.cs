using MediatR;

using ZeldaArena.Application.Common.Interfaces;
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
/// Проекция написана явным <c>Select</c>, а не <c>ProjectToType</c>, как в списке
/// турниров. Причина конкретная: Mapster разворачивает <c>TeamA.Name</c> без проверки
/// на null, потому что в DTO это ненулевая строка. В SQL это безразлично — внешний
/// ключ обязателен, и команда найдётся всегда, — но любой прогон того же выражения
/// не на EF Core падает с <c>NullReferenceException</c>, а значит хендлер становится
/// непроверяемым. Явное выражение переводится в SQL так же и при этом безопасно.
/// </summary>
public sealed class GetHomeMatchesQueryHandler(
    IReadRepository<Match> matches,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetHomeMatchesQuery, IReadOnlyList<HomeMatchDto>>
{
    public Task<IReadOnlyList<HomeMatchDto>> Handle(
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
            .Select(match => new HomeMatchDto
            {
                Id = match.Id,
                TeamAName = match.TeamA == null ? string.Empty : match.TeamA.Name,
                TeamALogoPath = match.TeamA == null ? null : match.TeamA.LogoPath,
                TeamBName = match.TeamB == null ? string.Empty : match.TeamB.Name,
                TeamBLogoPath = match.TeamB == null ? null : match.TeamB.LogoPath,
                ScoreA = match.ScoreA,
                ScoreB = match.ScoreB,
                ScheduledAt = match.ScheduledAt,
                Status = match.Status,
            });

        return queryExecutor.ToListAsync(query, cancellationToken);
    }
}