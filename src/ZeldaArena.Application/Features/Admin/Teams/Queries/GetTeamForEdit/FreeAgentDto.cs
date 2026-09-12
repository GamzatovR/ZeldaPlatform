using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Teams.Queries.GetTeamForEdit;

public sealed record FreeAgentDto(Guid Id, string Nickname, PlayerRole Role);