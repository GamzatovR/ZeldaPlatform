namespace ZeldaArena.Infrastructure.Payments;

/// <summary>
/// Настройки кода подтверждения оплаты (docs/SPEC.md §7.6).
/// </summary>
public sealed class ConfirmationCodeOptions
{
    public const string SectionName = "Payments:ConfirmationCode";

    /// <summary>
    /// Секрет, подмешиваемый в хеш. Шестизначный код — это миллион вариантов:
    /// голый SHA-256 по украденному дампу базы перебирается за секунды, а с перцем,
    /// который в дамп не попадает, перебор становится бессмысленным.
    ///
    /// Хранится в User Secrets и переменных окружения, в репозиторий не попадает
    /// (docs/SPEC.md §16).
    /// </summary>
    public string Pepper { get; set; } = string.Empty;
}