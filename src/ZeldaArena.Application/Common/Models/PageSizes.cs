namespace ZeldaArena.Application.Common.Models;

/// <summary>
/// Допустимые размеры страницы (docs/SPEC.md §10.3). Размер приходит из query-string,
/// то есть от пользователя, поэтому это whitelist, а не подсказка: произвольное число
/// в <c>Take</c> — это приглашение выгрузить таблицу целиком одним запросом.
/// </summary>
public static class PageSizes
{
    public const int Default = 12;

    public static readonly IReadOnlyList<int> Allowed = [12, 24, 48];

    /// <summary>
    /// Приводит запрошенный размер к разрешённому. Неизвестное значение заменяется
    /// на <see cref="Default"/>, а не отвергается ошибкой: ссылка с чужим pageSize
    /// должна открыться, просто с обычной страницей.
    /// </summary>
    public static int Normalize(int requested) =>
        Allowed.Contains(requested) ? requested : Default;
}