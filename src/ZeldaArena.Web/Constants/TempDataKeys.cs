namespace ZeldaArena.Web.Constants;

/// <summary>
/// Ключи <c>TempData</c>. Сообщение об исходе действия переживает редирект (PRG)
/// и читается одним partial'ом на всех страницах: разойдись ключ между записью
/// и чтением — сообщение молча пропало бы, без ошибки сборки и без исключения.
/// </summary>
public static class TempDataKeys
{
    /// <summary>
    /// Сообщение об исходе действия. Ведущий «!» — признак ошибки
    /// (<c>_StatusMessage.cshtml</c>).
    /// </summary>
    public const string StatusMessage = "StatusMessage";
}