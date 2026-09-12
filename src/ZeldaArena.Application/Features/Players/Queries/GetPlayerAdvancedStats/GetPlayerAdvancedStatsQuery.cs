using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Players.Queries.GetPlayerAdvancedStats;

public sealed record GetPlayerAdvancedStatsQuery(Guid PlayerId) : IQuery<Result<IReadOnlyList<PlayerTournamentStatsDto>>>;