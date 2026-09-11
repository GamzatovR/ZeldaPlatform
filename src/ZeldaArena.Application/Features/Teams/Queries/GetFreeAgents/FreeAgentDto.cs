using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Teams.Queries.GetFreeAgents;

public sealed record FreeAgentDto(Guid Id, string Nickname, PlayerRole Role, string CountryCode);