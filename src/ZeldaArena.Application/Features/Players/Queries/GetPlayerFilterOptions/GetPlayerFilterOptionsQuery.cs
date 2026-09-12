using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Players.Queries.GetPlayerFilterOptions;

public sealed record GetPlayerFilterOptionsQuery : IQuery<PlayerFilterOptionsDto>;