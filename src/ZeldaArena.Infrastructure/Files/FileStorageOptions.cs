namespace ZeldaArena.Infrastructure.Files;

/// <summary>
/// Настройки локального хранилища файлов из раздела <c>Storage:Files</c>.
/// </summary>
public sealed class FileStorageOptions
{
    public const string SectionName = "Storage:Files";

    /// <summary>
    /// Каталог хранилища. Относительный путь считается от корня содержимого приложения.
    /// По умолчанию — <c>App_Data/uploads</c>: вне <c>wwwroot</c>, чтобы веб-сервер
    /// не отдал загруженное напрямую, минуя проверки (docs/SPEC.md §15).
    /// </summary>
    public string RootPath { get; set; } = Path.Combine("App_Data", "uploads");
}