using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Application.Features.Admin.Tournaments;

/// <summary>
/// Регламент турнира на входе (docs/SPEC.md §15): пустой — это отсутствие регламента,
/// всё остальное проходит санитайзер. Одно место для создания и правки: пропусти
/// очистку один из двух сценариев — и <c>Html.Raw</c> на странице турнира перестал бы
/// быть безопасным.
/// </summary>
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