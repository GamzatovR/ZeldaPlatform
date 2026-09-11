using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Tournaments.Queries.GetTournamentOptions;

/// <summary>
/// Турниры для выпадающего списка фильтра — расписания и списка игроков. Только слаг
/// и название: фильтр адресует турнир слагом, как и ссылка на его страницу.
/// </summary>
public sealed record GetTournamentOptionsQuery : IQuery<IReadOnlyList<TournamentOptionDto>>;