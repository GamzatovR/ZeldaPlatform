using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;

using ZeldaArena.Web.Constants;

namespace ZeldaArena.Web.TagHelpers;

/// <summary>Адрес состояния списка.</summary>
internal static class ListUrl
{
    /// <param name="viewContext">Контекст отрисовки: из него берутся путь и текущие параметры.</param>
    /// <param name="changes">Параметр → новое значение; <see langword="null"/> убирает параметр.</param>
    public static string Build(ViewContext viewContext, IReadOnlyDictionary<string, string?> changes)
    {
        ArgumentNullException.ThrowIfNull(viewContext);
        ArgumentNullException.ThrowIfNull(changes);

        var request = viewContext.HttpContext.Request;
        var path = viewContext.ViewData[ListViewData.PagePath] as string
            ?? request.PathBase + request.Path;

        var query = request.Query
            .Where(pair => !changes.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);

        foreach (var (name, value) in changes)
        {
            if (value is not null)
            {
                query[name] = new StringValues(value);
            }
        }

        return path + QueryString.Create(query);
    }
}