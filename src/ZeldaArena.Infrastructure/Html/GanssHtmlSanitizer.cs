using Ganss.Xss;

namespace ZeldaArena.Infrastructure.Html;

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