using System.Globalization;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ZeldaArena.Web.TagHelpers;

/// <summary>Логотип команды или аватар игрока, а без картинки — инициалы.</summary>
[HtmlTargetElement("avatar", TagStructure = TagStructure.WithoutEndTag)]
public sealed class AvatarTagHelper : TagHelper
{
    private const int MaxInitials = 2;

    [HtmlAttributeName("image")]
    public string? Image { get; set; }

    [HtmlAttributeName("name")]
    public string Name { get; set; } = string.Empty;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var declared = output.Attributes["class"]?.Value?.ToString();
        output.Attributes.SetAttribute("class", string.IsNullOrWhiteSpace(declared)
            ? "entity-card__image"
            : $"entity-card__image {declared}");

        if (!string.IsNullOrEmpty(Image))
        {
            var image = new TagBuilder("img") { TagRenderMode = TagRenderMode.SelfClosing };
            image.Attributes["src"] = Image;
            image.Attributes["alt"] = string.Empty;
            image.Attributes["loading"] = "lazy";
            output.Content.SetHtmlContent(image);

            return;
        }

        var initials = new TagBuilder("span");
        initials.AddCssClass("entity-card__initials");
        initials.Attributes["aria-hidden"] = "true";
        initials.InnerHtml.Append(Initials(Name));
        output.Content.SetHtmlContent(initials);
    }

    private static string Initials(string name)
    {
        var letters = name
            .Split([' ', '-', '_'], StringSplitOptions.RemoveEmptyEntries)
            .Take(MaxInitials)
            .Select(word => char.ToUpper(word[0], CultureInfo.CurrentCulture));

        return string.Concat(letters);
    }
}