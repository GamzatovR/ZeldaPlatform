using System.Text;
using System.Text.Encodings.Web;

namespace ZeldaArena.Web.Services;

/// <summary>
/// Одна разметка на все письма аккаунта: приветствие, текст, кнопка действия
/// и та же ссылка обычным текстом.
///
/// Стили заданы прямо в атрибутах: почтовые клиенты в массе своей вырезают
/// внешние таблицы стилей, и вынос оформления в отдельный файл дал бы письмо
/// без оформления.
///
/// Всё, что приходит извне, кодируется: адрес и отображаемое имя задаёт пользователь,
/// и без экранирования письмо стало бы точкой внедрения разметки (docs/SPEC.md §15).
/// </summary>
internal static class EmailLayout
{
    public static string Render(
        string greeting,
        string body,
        string? actionText,
        string? actionUrl,
        string linkHint,
        string footer)
    {
        var html = new StringBuilder();

        html.Append(
            """
            <div style="font-family:Segoe UI,Arial,sans-serif;font-size:15px;color:#1b1b1f;max-width:560px">
            """);

        html.Append($"<p>{Encode(greeting)}</p>");
        html.Append($"<p>{Encode(body)}</p>");

        if (actionText is not null && actionUrl is not null)
        {
            html.Append(
                $"""
                <p><a href="{Encode(actionUrl)}" style="display:inline-block;padding:12px 20px;background:#7b2ff7;color:#fff;text-decoration:none;border-radius:6px">{Encode(actionText)}</a></p>
                <p style="font-size:13px;color:#5b5b66">{Encode(linkHint)}<br /><span>{Encode(actionUrl)}</span></p>
                """);
        }

        html.Append($"""<p style="font-size:13px;color:#5b5b66">{Encode(footer)}</p>""");
        html.Append("</div>");

        return html.ToString();
    }

    private static string Encode(string value) => HtmlEncoder.Default.Encode(value);
}