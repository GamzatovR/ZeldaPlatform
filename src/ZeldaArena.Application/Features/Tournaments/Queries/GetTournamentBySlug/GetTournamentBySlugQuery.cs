using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Tournaments.Queries.GetTournamentBySlug;

/// <summary>
/// Страница турнира (docs/SPEC.md §9.3, п. 4): описание, регламент и участники.
/// Матчи турнира — отдельный запрос с вкладками и пагинацией, потому что вкладки
/// подгружаются сами по себе (§10.1, сценарий 2).
/// </summary>
/// <param name="Slug">Слаг из адреса <c>/tournaments/{slug}</c>.</param>
public sealed record GetTournamentBySlugQuery(string Slug) : IQuery<TournamentDetailsDto?>;