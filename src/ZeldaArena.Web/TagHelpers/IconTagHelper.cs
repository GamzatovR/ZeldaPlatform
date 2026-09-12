using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

using ZeldaArena.Web.Constants;

namespace ZeldaArena.Web.TagHelpers;

/// <summary>Иконка из спрайта wwwroot/img/icons.svg.</summary>
[HtmlTargetElement("icon", Attributes = NameAttributeName, TagStructure = TagStructure.WithoutEndTag)]
public sealed class IconTagHelper : TagHelper
{
    private const string NameAttributeName = "name";

    [HtmlAttributeName(NameAttributeName)]
    public string Name { get; set; } = string.Empty;

    [HtmlAttributeName("label")]
    public string? Label { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        output.TagName = "svg";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("class", CssClass(output));
        output.Attributes.SetAttribute("focusable", "false");

        if (string.IsNullOrWhiteSpace(Label))
        {
            output.Attributes.SetAttribute("aria-hidden", "true");
        }
        else
        {
            output.Attributes.SetAttribute("role", "img");
            output.Attributes.SetAttribute("aria-label", Label);
        }

        // Спрайт адресуется от корня приложения, поэтому ссылка одинакова на любой глубине маршрута.
        var use = new TagBuilder("use") { TagRenderMode = TagRenderMode.SelfClosing };
        use.Attributes["href"] = ViewContext.HttpContext.Request.PathBase + IconNames.SpritePath + "#" + Name;

        output.Content.SetHtmlContent(use);
    }

    private string CssClass(TagHelperOutput output)
    {
        // Класс из разметки не затирается: <icon class="icon--lg" /> должен
        // и остаться крупным, и получить базовый .icon.
        var declared = output.Attributes["class"]?.Value?.ToString();

        return string.IsNullOrWhiteSpace(declared) ? "icon" : $"icon {declared}";
    }
}