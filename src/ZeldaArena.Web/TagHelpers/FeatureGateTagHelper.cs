using System.Security.Claims;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Web.TagHelpers;

/// <summary>Третий уровень проверки платных функций — разметка.</summary>
[HtmlTargetElement("feature-gate", Attributes = FeatureAttributeName)]
public sealed class FeatureGateTagHelper(IEntitlementService entitlements) : TagHelper
{
    private const string FeatureAttributeName = "feature";

    [HtmlAttributeName(FeatureAttributeName)]
    public string Feature { get; set; } = string.Empty;

    [HtmlAttributeName("when-missing")]
    public bool WhenMissing { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        // Сам элемент в разметку не попадает — остаётся только его содержимое.
        output.TagName = null;

        var granted = await HasFeatureAsync().ConfigureAwait(false);

        if (granted != WhenMissing)
        {
            return;
        }

        output.SuppressOutput();
    }

    private async Task<bool> HasFeatureAsync()
    {
        var user = ViewContext.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true
            || !Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            || string.IsNullOrWhiteSpace(Feature))
        {
            return false;
        }

        return await entitlements
            .HasFeatureAsync(userId, Feature, ViewContext.HttpContext.RequestAborted)
            .ConfigureAwait(false);
    }
}