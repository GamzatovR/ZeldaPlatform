using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Common.Models.Esports;

/// <summary>
/// Карточка матча — одна на все списки: главная, вкладки турнира, расписание,
/// история команды и игрока. Плоский набор примитивов: проекция уезжает в SQL целиком
/// и тянет ровно те столбцы, которые нужны разметке (docs/SPEC.md §16).
///
/// Одна карточка, а не своя на каждую страницу: в Фазе 10 счёт в ней начнёт
/// обновляться по SignalR (§11), и разметка, которую ищет клиент, обязана быть
/// одинаковой везде, где матч показан.
/// </summary>
public sealed record MatchCardDto
{
    public Guid Id { get; init; }

    public string TournamentName { get; init; } = string.Empty;

    public string TournamentSlug { get; init; } = string.Empty;

    public Guid TeamAId { get; init; }

    public string TeamAName { get; init; } = string.Empty;

    public string TeamASlug { get; init; } = string.Empty;

    public string? TeamALogoPath { get; init; }

    public Guid TeamBId { get; init; }

    public string TeamBName { get; init; } = string.Empty;

    public string TeamBSlug { get; init; } = string.Empty;

    public string? TeamBLogoPath { get; init; }

    public int ScoreA { get; init; }

    public int ScoreB { get; init; }

    public int BestOf { get; init; }

    public DateTimeOffset ScheduledAt { get; init; }

    public MatchStatus Status { get; init; }

    public Guid? WinnerTeamId { get; init; }

    /// <summary>
    /// Идёт прямо сейчас. Такой матч показывается первым и получает метку live,
    /// а в Фазе 10 — обновление счёта через SignalR (§11).
    /// </summary>
    public bool IsLive => Status == MatchStatus.Live;
}