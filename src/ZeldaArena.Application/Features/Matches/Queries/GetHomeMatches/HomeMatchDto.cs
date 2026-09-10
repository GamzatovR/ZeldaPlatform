using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Matches.Queries.GetHomeMatches;

/// <summary>
/// Карточка матча на главной. Плоский набор примитивов: проекция уезжает в SQL
/// целиком и тянет ровно те столбцы, которые нужны разметке (docs/SPEC.md §16).
/// </summary>
public sealed record HomeMatchDto
{
    public Guid Id { get; init; }

    public string TeamAName { get; init; } = string.Empty;

    public string? TeamALogoPath { get; init; }

    public string TeamBName { get; init; } = string.Empty;

    public string? TeamBLogoPath { get; init; }

    public int ScoreA { get; init; }

    public int ScoreB { get; init; }

    public DateTimeOffset ScheduledAt { get; init; }

    public MatchStatus Status { get; init; }

    /// <summary>
    /// Идёт прямо сейчас. Такой матч показывается первым и получает метку live,
    /// а в Фазе 10 — обновление счёта через SignalR (§11).
    /// </summary>
    public bool IsLive => Status == MatchStatus.Live;
}