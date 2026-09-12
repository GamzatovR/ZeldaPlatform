using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Teams.Queries.GetMyTeam;

public sealed record GetMyTeamQuery(Guid? TeamId = null) : IQuery<MyTeamDto?>;