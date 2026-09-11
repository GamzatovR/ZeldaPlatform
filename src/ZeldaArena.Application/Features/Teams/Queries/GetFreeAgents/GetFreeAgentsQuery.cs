using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Teams.Queries.GetFreeAgents;

/// <summary>
/// Свободные игроки — без открытой записи состава — для выпадающего списка
/// «взять в состав» в кабинете капитана.
/// </summary>
public sealed record GetFreeAgentsQuery : IQuery<IReadOnlyList<FreeAgentDto>>;