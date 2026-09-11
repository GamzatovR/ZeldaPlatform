using System.Linq.Expressions;

using ZeldaArena.Domain.Esports;

namespace ZeldaArena.Application.Common.Models.Esports;

/// <summary>
/// Единственная проекция матча в карточку. Выражение, а не метод: EF Core переводит
/// его в SQL целиком, и названия команд с турниром приходят JOIN'ом в том же запросе,
/// а не отдельным запросом на строку — обязательная проверка на N+1 из docs/SPEC.md §16.
///
/// Навигационные свойства разворачиваются с проверкой на null. В SQL это безразлично:
/// внешние ключи обязательны. Но то же выражение исполняется в памяти unit-тестами,
/// и без проверки падает с <c>NullReferenceException</c> — ровно эта ошибка была поймана
/// в Фазе 5, когда проекцию строил Mapster (docs/PROGRESS.md).
/// </summary>
public static class MatchCardProjection
{
    public static readonly Expression<Func<Match, MatchCardDto>> Expression = match => new MatchCardDto
    {
        Id = match.Id,
        TournamentName = match.Tournament == null ? string.Empty : match.Tournament.Name,
        TournamentSlug = match.Tournament == null ? string.Empty : match.Tournament.Slug.Value,
        TeamAId = match.TeamAId,
        TeamAName = match.TeamA == null ? string.Empty : match.TeamA.Name,
        TeamASlug = match.TeamA == null ? string.Empty : match.TeamA.Slug.Value,
        TeamALogoPath = match.TeamA == null ? null : match.TeamA.LogoPath,
        TeamBId = match.TeamBId,
        TeamBName = match.TeamB == null ? string.Empty : match.TeamB.Name,
        TeamBSlug = match.TeamB == null ? string.Empty : match.TeamB.Slug.Value,
        TeamBLogoPath = match.TeamB == null ? null : match.TeamB.LogoPath,
        ScoreA = match.ScoreA,
        ScoreB = match.ScoreB,
        BestOf = match.BestOf,
        ScheduledAt = match.ScheduledAt,
        Status = match.Status,
        WinnerTeamId = match.WinnerTeamId,
    };
}