using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Features.Matches.Queries.GetMatchDetails;

/// <summary>
/// Четыре запроса на страницу, сколько бы игроков ни было: карточка матча, время
/// и трансляция, составы обеих команд одним запросом и статистика одним запросом.
/// Обязательная проверка на N+1 из docs/SPEC.md §16 — на странице матча её требуют отдельно.
///
/// Состав берётся на момент матча, а не текущий: состав историчен (docs/CONVENTIONS.md,
/// «Правила предметной области»), и ушедший после финала игрок в прошлом матче
/// обязан остаться в составе, а пришедший позже — не появиться в нём задним числом.
///
/// Игроки подтягиваются явным соединением, а не навигационным свойством: так тот же
/// запрос исполняется и в памяти, и unit-тесты проверяют выбор состава по датам,
/// а не пустые навигации.
/// </summary>
public sealed class GetMatchDetailsQueryHandler(
    IReadRepository<Match> matches,
    IReadRepository<RosterEntry> rosterEntries,
    IReadRepository<Player> players,
    IReadRepository<PlayerMatchStats> stats,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetMatchDetailsQuery, MatchDetailsDto?>
{
    public async Task<MatchDetailsDto?> Handle(GetMatchDetailsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var card = await queryExecutor.FirstOrDefaultAsync(
            matches.Query()
                .Where(match => match.Id == request.Id)
                .Select(MatchCardProjection.Expression),
            cancellationToken).ConfigureAwait(false);

        if (card is null)
        {
            return null;
        }

        // Отдельный узкий запрос, а не своя копия проекции карточки ради трёх полей:
        // карточка обязана быть одной на все страницы (MatchCardProjection).
        var header = await queryExecutor.FirstOrDefaultAsync(
            matches.Query()
                .Where(match => match.Id == request.Id)
                .Select(match => new { match.StartedAt, match.EndedAt, match.StreamUrl }),
            cancellationToken).ConfigureAwait(false);

        if (header is null)
        {
            return null;
        }

        // Точка отсчёта — начало матча, а у ещё не начатого — время по расписанию.
        var moment = header.StartedAt ?? card.ScheduledAt;

        var lineup = await queryExecutor.ToListAsync(
            from entry in rosterEntries.Query()
            join player in players.Query() on entry.PlayerId equals player.Id
            where (entry.TeamId == card.TeamAId || entry.TeamId == card.TeamBId)
                && entry.JoinedAt <= moment
                && (entry.LeftAt == null || entry.LeftAt > moment)
            orderby entry.Role, player.Nickname
            select new LineupPlayerDto
            {
                PlayerId = player.Id,
                TeamId = entry.TeamId,
                Slug = player.Slug.Value,
                Nickname = player.Nickname,
                Role = entry.Role,
                CountryCode = player.Country.Value,
                AvatarPath = player.AvatarPath,
            },
            cancellationToken).ConfigureAwait(false);

        var playerStats = await queryExecutor.ToListAsync(
            from line in stats.Query()
            join player in players.Query() on line.PlayerId equals player.Id
            where line.MatchId == request.Id
            orderby line.Rating descending, player.Nickname
            select new PlayerMatchStatsDto
            {
                PlayerId = player.Id,
                TeamId = line.TeamId,
                Slug = player.Slug.Value,
                Nickname = player.Nickname,
                Kills = line.Kills,
                Deaths = line.Deaths,
                Assists = line.Assists,
                Damage = line.Damage,
                Rating = line.Rating,
            },
            cancellationToken).ConfigureAwait(false);

        return new MatchDetailsDto
        {
            Card = card,
            StartedAt = header.StartedAt,
            EndedAt = header.EndedAt,
            StreamUrl = IsWebAddress(header.StreamUrl) ? header.StreamUrl : null,
            LineupA = [.. lineup.Where(player => player.TeamId == card.TeamAId)],
            LineupB = [.. lineup.Where(player => player.TeamId == card.TeamBId)],
            StatsA = [.. playerStats.Where(line => line.TeamId == card.TeamAId)],
            StatsB = [.. playerStats.Where(line => line.TeamId == card.TeamBId)],
        };
    }

    private static bool IsWebAddress(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}