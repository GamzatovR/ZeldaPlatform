using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Features.Teams.Queries.GetTeamAdvancedStats;

public sealed record GetTeamAdvancedStatsQuery(Guid TeamId) : IQuery<Result<TeamAdvancedStatsDto>>;