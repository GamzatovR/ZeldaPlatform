using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Features.News.Queries.GetLatestNews;
using ZeldaArena.Application.Features.Shop.Queries.GetProducts;
using ZeldaArena.Application.Features.Teams.Queries.GetTeams;

namespace ZeldaArena.Web.Models.Home;

/// <summary>
/// Главная страница (docs/SPEC.md §9.3, страница 1). Собирается контроллером
/// из четырёх запросов; вьюха ничего не вычисляет.
///
/// Бегущая строка live-матчей из §9.3 придёт в Фазе 10 вместе со своим сценарием.
/// Пустого поля под неё здесь нет сознательно: заглушка, притворяющаяся данными,
/// хуже отсутствующей секции.
/// </summary>
public sealed class HomeViewModel
{
    public required IReadOnlyList<MatchCardDto> Matches { get; init; }

    public required IReadOnlyList<NewsListItemDto> News { get; init; }

    public required IReadOnlyList<TeamListItemDto> TopTeams { get; init; }

    public required IReadOnlyList<ProductListItemDto> Products { get; init; }
}