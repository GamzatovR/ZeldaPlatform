using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ZeldaArena.Web.TagHelpers;

/// <summary>
/// Подсветка текущего пункта меню (docs/SPEC.md §10.2, <c>&lt;active-route&gt;</c>):
///
/// <code>
/// &lt;a asp-controller="Shop" asp-action="Index" active-controller="Shop"&gt;Магазин&lt;/a&gt;
/// </code>
///
/// Совпадение добавляет класс <c>is-active</c> и, что важнее, атрибут
/// <c>aria-current="page"</c>: подсветка цветом видна только зрячим, а §9.1
/// требует, чтобы текущий раздел был различим и в скринридере.
///
/// Сравнение идёт по данным маршрута, а не по строке адреса: путь ломается
/// от лишнего слэша, query-string фильтра и языкового префикса из Фазы 11.
/// </summary>
[HtmlTargetElement(Attributes = ControllerAttributeName)]
[HtmlTargetElement(Attributes = PageAttributeName)]
public sealed class ActiveRouteTagHelper : TagHelper
{
    private const string ControllerAttributeName = "active-controller";
    private const string ActionAttributeName = "active-action";
    private const string PageAttributeName = "active-page";
    private const string ActiveClass = "is-active";

    /// <summary>Контроллер, на котором пункт считается текущим.</summary>
    [HtmlAttributeName(ControllerAttributeName)]
    public string? Controller { get; set; }

    /// <summary>
    /// Действие. Не задано — подсвечивается весь раздел: страница товара
    /// обязана подсвечивать «Магазин».
    /// </summary>
    [HtmlAttributeName(ActionAttributeName)]
    public string? Action { get; set; }

    /// <summary>Razor Page, на которой пункт считается текущим (область Identity).</summary>
    [HtmlAttributeName(PageAttributeName)]
    public string? Page { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        // Служебные атрибуты в разметку не уезжают.
        output.Attributes.RemoveAll(ControllerAttributeName);
        output.Attributes.RemoveAll(ActionAttributeName);
        output.Attributes.RemoveAll(PageAttributeName);

        if (!IsActive())
        {
            return;
        }

        output.AddClass(ActiveClass, System.Text.Encodings.Web.HtmlEncoder.Default);
        output.Attributes.SetAttribute("aria-current", "page");
    }

    private bool IsActive()
    {
        var values = ViewContext.RouteData.Values;

        if (!string.IsNullOrEmpty(Page))
        {
            return Matches(values["page"], Page);
        }

        if (string.IsNullOrEmpty(Controller) || !Matches(values["controller"], Controller))
        {
            return false;
        }

        return string.IsNullOrEmpty(Action) || Matches(values["action"], Action);
    }

    private static bool Matches(object? routeValue, string expected) =>
        string.Equals(routeValue?.ToString(), expected, StringComparison.OrdinalIgnoreCase);
}
