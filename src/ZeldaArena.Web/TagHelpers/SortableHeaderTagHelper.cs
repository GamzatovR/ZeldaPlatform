using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;

using ZeldaArena.Web.Constants;

namespace ZeldaArena.Web.TagHelpers;

/// <summary>
/// Заголовок столбца с сортировкой по клику (docs/SPEC.md §9.2, <c>&lt;sortable-header&gt;</c>;
/// §9.4: «сортировка по клику на заголовок»):
///
/// <code>
/// &lt;sortable-header asc="name_asc" desc="name_desc" current="@sort" default="@Map.DefaultKey"&gt;Название&lt;/sortable-header&gt;
/// </code>
///
/// Ключи — те же, что в <c>SortMap</c> сценария: имя поля из адреса в выражение
/// сортировки не попадает никогда, неизвестный ключ сервер молча заменит умолчанием
/// (§10.2, whitelist). Столбец без одного из направлений задаёт только его.
///
/// Ссылка — текущий адрес с заменённым <c>sort</c> и сброшенной страницей: после
/// пересортировки третья страница — это уже другие строки. Без JavaScript это обычная
/// ссылка, с ним <c>ajax-list.js</c> перехватывает её по <c>data-sort</c>.
/// </summary>
[HtmlTargetElement("sortable-header")]
public sealed class SortableHeaderTagHelper(IStringLocalizer<SharedResource> localizer) : TagHelper
{
    public const string SortParameter = "sort";

    [HtmlAttributeName("asc")]
    public string? Ascending { get; set; }

    [HtmlAttributeName("desc")]
    public string? Descending { get; set; }

    /// <summary>Действующий ключ сортировки — уже нормализованный <c>SortMap.Resolve</c>.</summary>
    [HtmlAttributeName("current")]
    public string? Current { get; set; }

    /// <summary>Ключ по умолчанию: он в адрес не пишется, у одного состояния — один адрес.</summary>
    [HtmlAttributeName("default")]
    public string? Default { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        if (Ascending is null && Descending is null)
        {
            throw new InvalidOperationException("<sortable-header> требует asc или desc.");
        }

        var label = (await output.GetChildContentAsync()).GetContent();
        var direction = Current is not null && Current == Ascending ? "ascending"
            : Current is not null && Current == Descending ? "descending"
            : null;

        // Первый клик — по возрастанию, повторный — в обратную сторону.
        var target = direction switch
        {
            "ascending" => Descending ?? Ascending!,
            "descending" => Ascending ?? Descending!,
            _ => Ascending ?? Descending!,
        };

        output.TagName = "th";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("scope", "col");

        if (direction is not null)
        {
            // Состояние сортировки слышно и в скринридере, а не только видно по стрелке (§9.1).
            output.Attributes.SetAttribute("aria-sort", direction);
        }

        var link = new TagBuilder("a");
        link.AddCssClass("sortable-header");
        if (direction is not null)
        {
            link.AddCssClass("is-" + direction);
        }

        link.Attributes["href"] = ListUrl.Build(ViewContext, new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            [SortParameter] = target == Default ? null : target,
            [PaginationTagHelper.PageParameter] = null,
        });
        link.Attributes["data-sort"] = target;

        link.InnerHtml.AppendHtml(label);

        var hint = new TagBuilder("span");
        hint.AddCssClass("visually-hidden");
        hint.InnerHtml.Append(localizer[target == Descending ? "table.sort_descending" : "table.sort_ascending"].Value);
        link.InnerHtml.AppendHtml(hint);

        link.InnerHtml.AppendHtml(Icon());

        output.Content.SetHtmlContent(link);
        output.AddClass("sortable", HtmlEncoder.Default);
    }

    private TagBuilder Icon()
    {
        var svg = new TagBuilder("svg");
        svg.AddCssClass("icon");
        svg.AddCssClass("sortable-header__icon");
        svg.Attributes["aria-hidden"] = "true";
        svg.Attributes["focusable"] = "false";

        var use = new TagBuilder("use") { TagRenderMode = TagRenderMode.SelfClosing };
        use.Attributes["href"] = ViewContext.HttpContext.Request.PathBase + IconNames.SpritePath + "#" + IconNames.ChevronDown;
        svg.InnerHtml.AppendHtml(use);

        return svg;
    }
}