namespace ZeldaArena.Domain.Constants;

/// <summary>
/// Языки интерфейса: ru по умолчанию и en (docs/SPEC.md §9.5).
///
/// Список нужен не только представлению: выбранный язык хранится в профиле
/// пользователя и в переводах контента (ContentTranslations), поэтому проверять
/// его приходится и на сервере, в валидаторах Application.
///
/// Новый язык (EP-7) добавляется строкой сюда, файлом .resx и кодом культуры
/// в конфигурации — ядра это не касается.
/// </summary>
public static class SupportedCultures
{
    public const string Russian = "ru";

    public const string English = "en";

    public const string Default = Russian;

    public static IReadOnlyList<string> All { get; } = [Russian, English];

    public static bool IsSupported(string? cultureCode) =>
        cultureCode is not null
        && All.Contains(cultureCode, StringComparer.OrdinalIgnoreCase);
}