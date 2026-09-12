using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Application.Features.Admin.Tournaments;

internal static class TournamentRulesHtml
{
    public static string? Sanitize(IHtmlSanitizer sanitizer, string? html)
    {
        ArgumentNullException.ThrowIfNull(sanitizer);

        if (string.IsNullOrWhiteSpace(html))
        {
            return null;
        }

        var clean = sanitizer.Sanitize(html);

        return string.IsNullOrWhiteSpace(clean) ? null : clean;
    }
}