using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Matches.Queries.GetMatchDetails;

/// <summary>
/// Страница матча (docs/SPEC.md §9.3, п. 5): счёт, составы на момент матча и статистика
/// игроков. Лента событий и чат — Фаза 10 вместе с MongoDB и SignalR; комментарии — Приоритет B (§17), не делаются.
/// </summary>
public sealed record GetMatchDetailsQuery(Guid Id) : IQuery<MatchDetailsDto?>;