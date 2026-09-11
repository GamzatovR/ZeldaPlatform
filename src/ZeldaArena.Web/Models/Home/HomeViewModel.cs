using ZeldaArena.Application.Common.Models.Esports;
using ZeldaArena.Application.Features.News.Queries.GetLatestNews;

namespace ZeldaArena.Web.Models.Home;

/// <summary>
/// Главная страница (docs/SPEC.md §9.3, страница 1). Собирается контроллером
/// из двух запросов; вьюха ничего не вычисляет.
///
/// Витрина товаров, топ-команды и бегущая строка live-матчей из §9.3 сюда придут
/// в Фазах 7 и 10 вместе со своими сценариями. Пустых полей под них здесь нет
/// сознательно: заглушка, притворяющаяся данными, хуже отсутствующей секции.
/// </summary>
public sealed class HomeViewModel
{
    public required IReadOnlyList<MatchCardDto> Matches { get; init; }

    public required IReadOnlyList<NewsListItemDto> News { get; init; }
}