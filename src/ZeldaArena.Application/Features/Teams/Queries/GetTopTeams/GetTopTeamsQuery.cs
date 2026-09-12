using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Teams.Queries.GetTeams;

namespace ZeldaArena.Application.Features.Teams.Queries.GetTopTeams;

public sealed record GetTopTeamsQuery(int Count = 4) : IQuery<IReadOnlyList<TeamListItemDto>>;