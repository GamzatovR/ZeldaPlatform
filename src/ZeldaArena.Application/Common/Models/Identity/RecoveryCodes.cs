namespace ZeldaArena.Application.Common.Models.Identity;

/// <summary>
/// Одноразовые коды восстановления доступа (docs/SPEC.md §8.2). Показываются
/// пользователю ровно один раз, в базе хранятся Identity и повторно не выдаются.
///
/// Обёртка вокруг списка нужна ради имени: свойство RecoveryCodes попадает под маску
/// SensitiveProperties, а безымянный IReadOnlyList&lt;string&gt; в логах не закрылся бы.
/// </summary>
public sealed record RecoveryCodes(IReadOnlyList<string> Codes)
{
    public static readonly RecoveryCodes Empty = new([]);
}