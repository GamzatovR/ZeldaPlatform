using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.Application.Common.Interfaces;

public interface IMatchEventStore
{
    Task AppendAsync(MatchEventRecord matchEvent, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MatchEventRecord>> GetByMatchAsync(
        Guid matchId,
        CancellationToken cancellationToken = default);
}