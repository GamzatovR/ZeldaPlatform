using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Matches.Queries.GetHomeMatches;

/// <summary>
/// Матчи для главной страницы (docs/SPEC.md §9.3, страница 1): сначала идущие,
/// затем ближайшие запланированные.
///
/// Это не список матчей с фильтром — тот появится в Фазе 6 вместе со страницей
/// турнира. Здесь нужен короткий срез витрины, поэтому ни фильтра, ни сортировки
/// у запроса нет: их нечем было бы выразить в ссылке, а значит им здесь не место
/// (§10.2, источник истины о состоянии списка — URL).
/// </summary>
/// <param name="Count">Сколько матчей показать. Ограничивается валидатором.</param>
public sealed record GetHomeMatchesQuery(int Count = 4) : IQuery<IReadOnlyList<HomeMatchDto>>;