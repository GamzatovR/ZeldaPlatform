using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Teams.Queries.GetTeamBySlug;

/// <summary>
/// Страница команды (docs/SPEC.md §9.3, п. 7): профиль, базовая статистика, форма,
/// состав и история матчей. Расширенная статистика — отдельный запрос за фичей
/// <c>stats.advanced</c>.
/// </summary>
public sealed record GetTeamBySlugQuery(string Slug) : IQuery<TeamDetailsDto?>;