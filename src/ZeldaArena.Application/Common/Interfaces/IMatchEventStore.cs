using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Лента событий матча (docs/SPEC.md §12, коллекция <c>match_events</c> в MongoDB).
/// Реализация — Фаза 10. Драйвер MongoDB за пределы Infrastructure не выходит.
/// </summary>
public interface IMatchEventStore
{
    Task AppendAsync(MatchEventRecord matchEvent, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MatchEventRecord>> GetByMatchAsync(
        Guid matchId,
        CancellationToken cancellationToken = default);
}