using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;

using ZeldaArena.Web.Constants;

namespace ZeldaArena.Web.TagHelpers;

/// <summary>
/// Единая пагинация всех списков (docs/SPEC.md §10.3, <c>&lt;pagination&gt;</c>):
///
/// <code>
/// &lt;pagination page="@Model.Result.Page" total-pages="@Model.Result.TotalPages" /&gt;
/// </code>
///
/// Каждая ссылка — полный адрес текущей страницы, в котором заменён только номер:
/// фильтр и сортировка сохраняются, а ссылку на третью страницу можно переслать
/// и открыть в новой вкладке (§10.2, «источник истины — URL»). Без JavaScript
/// это обычные ссылки; с ним <c>ajax-list.js</c> перехватывает их по
/// <c>data-pagination</c> и подгружает partial с <c>history.pushState</c>.
/// </summary>
[HtmlTargetElement("pagination", TagStructure = TagStructure.WithoutEndTag)]
public sealed class PaginationTagHelper(IStringLocalizer<SharedResource> localizer) : TagHelper
{
    public const string PageParameter = "page";

    /// <summary>Сколько соседей текущей страницы показывать с каждой стороны.</summary>
    private const int Neighbours = 2;

    [HtmlAttributeName("page")]
    public int Page { get; set; } = 1;

    [HtmlAttributeName("total-pages")]
    public int TotalPages { get; set; } = 1;

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        // Одна страница — навигации нет: ряд из одной кнопки ничего не сообщает.
        if (TotalPages <= 1)
        {
            output.SuppressOutput();
            return;
        }

        var current = Math.Clamp(Page, 1, TotalPages);

        output.TagName = "nav";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("aria-label", localizer["pagination.label"].Value);
        output.Attributes.SetAttribute("data-pagination", string.Empty);

        var list = new TagBuilder("ul");
        list.AddCssClass("pagination-token");

        list.InnerHtml.AppendHtml(Arrow(current - 1, IconNames.ChevronLeft, "pagination.previous", "prev"));

        var previous = 0;
        foreach (var number in VisiblePages(current))
        {
            if (number - previous > 1)
            {
                list.InnerHtml.AppendHtml(Gap());
            }

            list.InnerHtml.AppendHtml(Number(number, current));
            previous = number;
        }

        list.InnerHtml.AppendHtml(Arrow(current + 1, IconNames.ChevronRight, "pagination.next", "next"));

        output.Content.SetHtmlContent(list);
    }

    /// <summary>Первая, последняя и соседи текущей; остальное схлопывается в многоточие.</summary>
    private IEnumerable<int> VisiblePages(int current) =>
        Enumerable.Range(1, TotalPages)
            .Where(number => number == 1
                || number == TotalPages
                || Math.Abs(number - current) <= Neighbours);

    private TagBuilder Number(int number, int current)
    {
        var link = Link(number);
        link.InnerHtml.Append(number.ToString(System.Globalization.CultureInfo.CurrentCulture));
        link.Attributes["aria-label"] = localizer["pagination.page", number].Value;

        if (number == current)
        {
            link.Attributes["aria-current"] = "page";
        }

        return Item(link);
    }

    private TagBuilder Arrow(int target, string icon, string labelKey, string rel)
    {
        TagBuilder control;

        if (target < 1 || target > TotalPages)
        {
            // Недоступная стрелка — не ссылка: у неё нет адреса, и фокус на неё не встаёт.
            control = new TagBuilder("span");
            control.AddCssClass("pagination-token__link");
            control.AddCssClass("is-disabled");
            control.Attributes["aria-hidden"] = "true";
        }
        else
        {
            control = Link(target);
            control.Attributes["rel"] = rel;
            control.Attributes["aria-label"] = localizer[labelKey].Value;
        }

        control.InnerHtml.AppendHtml(Icon(icon));

        return Item(control);
    }

    private static TagBuilder Gap()
    {
        var gap = new TagBuilder("span");
        gap.AddCssClass("pagination-token__gap");
        gap.Attributes["aria-hidden"] = "true";
        gap.InnerHtml.Append("…");

        return Item(gap);
    }

    private TagBuilder Link(int number)
    {
        var link = new TagBuilder("a");
        link.AddCssClass("pagination-token__link");
        link.Attributes["href"] = UrlFor(number);
        link.Attributes["data-page"] = number.ToString(System.Globalization.CultureInfo.InvariantCulture);

        return link;
    }

    /// <summary>
    /// Текущий адрес с заменённым номером. Первая страница — без параметра: у одного
    /// состояния списка должен быть один адрес, иначе закладки и кэш двоятся.
    ///
    /// Список, отданный из Areas/Api, рисуется в ответ на <c>/api/…</c>; путь страницы
    /// тогда приходит в <see cref="ListViewData.PagePath"/>, а параметры фильтра —
    /// те же, что у запроса.
    /// </summary>
    private string UrlFor(int number)
    {
        var request = ViewContext.HttpContext.Request;
        var path = ViewContext.ViewData[ListViewData.PagePath] as string
            ?? request.PathBase + request.Path;
        var query = request.Query
            .Where(pair => !string.Equals(pair.Key, PageParameter, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);

        if (number > 1)
        {
            query[PageParameter] = new StringValues(number.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        return path + QueryString.Create(query);
    }

    private TagBuilder Icon(string name)
    {
        var svg = new TagBuilder("svg");
        svg.AddCssClass("icon");
        svg.Attributes["aria-hidden"] = "true";
        svg.Attributes["focusable"] = "false";

        var use = new TagBuilder("use") { TagRenderMode = TagRenderMode.SelfClosing };
        use.Attributes["href"] = ViewContext.HttpContext.Request.PathBase + IconNames.SpritePath + "#" + name;
        svg.InnerHtml.AppendHtml(use);

        return svg;
    }

    private static TagBuilder Item(TagBuilder content)
    {
        var item = new TagBuilder("li");
        item.InnerHtml.AppendHtml(content);

        return item;
    }
}