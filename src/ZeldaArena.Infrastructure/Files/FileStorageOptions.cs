namespace ZeldaArena.Infrastructure.Files;

/// <summary>Настройки локального хранилища файлов из раздела Storage:Files.</summary>
public sealed class FileStorageOptions
{
    public const string SectionName = "Storage:Files";

    public string RootPath { get; set; } = Path.Combine("App_Data", "uploads");
}