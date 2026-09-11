namespace ZeldaArena.Web.Constants;

/// <summary>
/// Ключи <c>ViewData</c>, которыми эндпоинт списка сообщает partial-представлению
/// то, чего нельзя узнать из текущего запроса.
/// </summary>
public static class ListViewData
{
    /// <summary>
    /// Адрес страницы, которой принадлежит список. Partial, отданный из <c>Areas/Api</c>,
    /// рисуется в ответ на <c>/api/tournaments</c>, но ссылки пагинации в нём обязаны
    /// вести на <c>/tournaments?page=2</c>: их открывают в новой вкладке и пересылают
    /// (docs/SPEC.md §10.3).
    /// </summary>
    public const string PagePath = "List.PagePath";
}
