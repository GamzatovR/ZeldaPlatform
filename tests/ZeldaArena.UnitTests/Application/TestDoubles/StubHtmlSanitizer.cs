using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

internal sealed class StubHtmlSanitizer : IHtmlSanitizer
{
    public const string Mark = "<!--sanitized-->";

    public List<string> Inputs { get; } = [];

    public string Sanitize(string html)
    {
        Inputs.Add(html);

        var withoutScripts = System.Text.RegularExpressions.Regex.Replace(
            html, "<script.*?</script>", string.Empty, System.Text.RegularExpressions.RegexOptions.Singleline);

        return withoutScripts.Length == 0 ? string.Empty : Mark + withoutScripts;
    }
}