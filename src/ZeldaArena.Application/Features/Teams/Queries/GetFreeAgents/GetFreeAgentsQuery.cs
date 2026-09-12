using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Teams.Queries.GetFreeAgents;

public sealed record GetFreeAgentsQuery : IQuery<IReadOnlyList<FreeAgentDto>>;