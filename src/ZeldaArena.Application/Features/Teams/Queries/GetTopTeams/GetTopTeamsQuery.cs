using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Teams.Queries.GetTeams;

namespace ZeldaArena.Application.Features.Teams.Queries.GetTopTeams;

/// <summary>
/// Топ-команды для главной (docs/SPEC.md §9.3, страница 1): лучшие по рейтингу.
/// Короткий срез витрины, как матчи главной, — без фильтра и пагинации.
/// </summary>
/// <param name="Count">Сколько команд показать. Ограничивается валидатором.</param>
public sealed record GetTopTeamsQuery(int Count = 4) : IQuery<IReadOnlyList<TeamListItemDto>>;