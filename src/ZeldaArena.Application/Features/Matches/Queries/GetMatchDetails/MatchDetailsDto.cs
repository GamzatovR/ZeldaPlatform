using ZeldaArena.Application.Common.Models.Esports;

namespace ZeldaArena.Application.Features.Matches.Queries.GetMatchDetails;

/// <summary>
/// Матч для его страницы. Счёт и команды — та же карточка, что в списках: в Фазе 10
/// клиент SignalR обновляет её одинаково везде, где матч показан (§11).
/// </summary>
public sealed record MatchDetailsDto
{
    public required MatchCardDto Card { get; init; }

    public DateTimeOffset? StartedAt { get; init; }

    public DateTimeOffset? EndedAt { get; init; }

    /// <summary>
    /// Ссылка на трансляцию — только абсолютная http(s). Адрес со схемой
    /// <c>javascript:</c> в атрибуте <c>href</c> исполнился бы по клику, и кодирование
    /// Razor от этого не спасает: оно защищает от выхода из атрибута, а не от схемы.
    /// </summary>
    public string? StreamUrl { get; init; }

    public IReadOnlyList<LineupPlayerDto> LineupA { get; init; } = [];

    public IReadOnlyList<LineupPlayerDto> LineupB { get; init; } = [];

    public IReadOnlyList<PlayerMatchStatsDto> StatsA { get; init; } = [];

    public IReadOnlyList<PlayerMatchStatsDto> StatsB { get; init; } = [];
}