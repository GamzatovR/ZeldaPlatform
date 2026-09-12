using Ganss.Xss;

namespace ZeldaArena.Infrastructure.Html;

/// <summary>
/// Реализация <see cref="Application.Common.Interfaces.IHtmlSanitizer"/> на Ganss.Xss.
///
/// Белый список уже умолчаний библиотеки: регламенту и новости нужны заголовки, абзацы,
/// списки, выделение, ссылки и простые таблицы. Картинок, форм, стилей и классов нет —
/// внешний вид задаёт <c>.prose</c> сайта, а не автор текста, и чем меньше разрешено,
/// тем меньше поверхность для подстановки разметки (docs/SPEC.md §15).
///
/// Ссылки — только http, https и mailto: <c>javascript:</c> в href — классический обход
/// санитайзера, который пропускает всё, что похоже на ссылку.
/// </summary>
public sealed class GanssHtmlSanitizer : Application.Common.Interfaces.IHtmlSanitizer
{
    private static readonly string[] AllowedTags =
    [
        "h2", "h3", "h4", "p", "br", "hr",
        "ul", "ol", "li",
        "strong", "b", "em", "i", "u", "s", "blockquote",
        "a",
        "table", "thead", "tbody", "tr", "th", "td",
    ];

    private readonly HtmlSanitizer _sanitizer = Create();

    public string Sanitize(string html)
    {
        ArgumentNullException.ThrowIfNull(html);

        return _sanitizer.Sanitize(html);
    }

    private static HtmlSanitizer Create()
    {
        var sanitizer = new HtmlSanitizer();

        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedTags.UnionWith(AllowedTags);

        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedAttributes.UnionWith(["href", "title", "colspan", "rowspan"]);

        sanitizer.AllowedCssProperties.Clear();
        sanitizer.AllowedAtRules.Clear();

        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.UnionWith(["http", "https", "mailto"]);

        // Внешняя ссылка из регламента не должна получать доступ к вкладке сайта.
        sanitizer.PostProcessNode += (_, args) =>
        {
            if (args.Node is AngleSharp.Dom.IElement { TagName: "A" } link)
            {
                link.SetAttribute("rel", "noopener nofollow");
            }
        };

        return sanitizer;
    }
}