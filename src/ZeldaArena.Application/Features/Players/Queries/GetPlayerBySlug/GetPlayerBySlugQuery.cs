using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Players.Queries.GetPlayerBySlug;

/// <summary>
/// Страница игрока (docs/SPEC.md §9.3, п. 10): профиль, текущая и прошлые команды,
/// статистика и последние матчи. Расширенная статистика — отдельный запрос за фичей.
/// </summary>
public sealed record GetPlayerBySlugQuery(string Slug) : IQuery<PlayerDetailsDto?>;