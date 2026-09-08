using ZeldaArena.Domain.Esports;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Seed;

/// <summary>
/// Десять новостей портала (docs/SPEC.md §6). В сид Фазы 1 они не попали: у новости
/// обязателен автор с внешним ключом на AspNetUsers, а пользователи появились
/// только в Фазе 3.
///
/// Разметка тела заведомо безопасная — те же теги, что пропускает HtmlSanitizer
/// на входе в Фазе 11 (§15). Это единственное место, где HTML попадает в базу
/// в обход санитайзера, и оно под контролем.
/// </summary>
public static class NewsSeedData
{
    /// <summary>Одна новость из десяти остаётся черновиком: список должен уметь их прятать.</summary>
    private const int DraftIndex = 9;

    public static IReadOnlyList<NewsArticle> Articles(Guid authorId, DateTimeOffset now)
    {
        var definitions = new (string Slug, string Title, string Summary, string Body)[]
        {
            ("hyrule-open-2026-anons", "Hyrule Open 2026: анонс главного турнира года",
                "Призовой фонд вырос вдвое, участвуют шестнадцать команд.",
                "Организаторы объявили даты и формат Hyrule Open 2026."),
            ("kakariko-guard-novyy-sostav", "Kakariko Guard объявила новый состав",
                "В команду пришли двое игроков из академии.",
                "Обновлённый состав дебютирует уже в групповом этапе."),
            ("zora-domain-pobeda", "Zora Domain берёт Triforce Cup",
                "Финал закончился со счётом 3:2 после карты овертайма.",
                "Матч продлился почти четыре часа и стал самым долгим в сезоне."),
            ("pravila-sezona", "Изменения в регламенте сезона",
                "Формат групп меняется с одинарного на двойной круг.",
                "Судейская коллегия опубликовала обновлённый регламент."),
            ("goron-city-transfer", "Goron City подписала капитана Lost Woods",
                "Трансфер закрыт за неделю до дедлайна.",
                "Сумма сделки не раскрывается."),
            ("rito-village-akademiya", "Rito Village открывает академию",
                "Набор идёт до конца месяца.",
                "Академия рассчитана на двенадцать игроков."),
            ("statistika-sezona", "Статистика сезона: кто набрал больше всех",
                "Разбор показателей по всем сорока матчам.",
                "Лидер по среднему рейтингу сменился трижды за сезон."),
            ("raspisanie-pleyoff", "Расписание плей-офф опубликовано",
                "Первые матчи стартуют в пятницу.",
                "Все встречи пройдут в студии организатора."),
            ("interv-yu-kapitana", "Интервью: капитан Hyrule Knights о подготовке",
                "О тренировках, составе и планах на финал.",
                "«Мы разбирали каждую карту соперника по минутам», — говорит капитан."),
            ("chernovik-o-magazine", "Обновление магазина периферии",
                "Черновик: материал ещё готовится к публикации.",
                "Текст дорабатывается редакцией."),
        };

        var articles = new List<NewsArticle>(definitions.Length);

        for (var index = 0; index < definitions.Length; index++)
        {
            var (slug, title, summary, body) = definitions[index];

            var article = NewsArticle.Draft(
                Slug.From(slug),
                title,
                $"<p>{body}</p>",
                authorId,
                summary);

            if (index != DraftIndex)
            {
                // Разные даты публикации нужны, чтобы сортировка ленты была видна.
                article.Publish(now.AddDays(-index * 3));
            }

            articles.Add(article);
        }

        return articles;
    }
}