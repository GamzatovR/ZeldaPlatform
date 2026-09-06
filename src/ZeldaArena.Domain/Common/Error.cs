namespace ZeldaArena.Domain.Common;

/// <summary>
/// Ошибка сценария. <see cref="Code"/> — ключ ресурса, а не готовый текст:
/// сообщение переводится на слое представления (docs/SPEC.md §9.5).
/// </summary>
/// <param name="Code">Ключ ресурса локализации, например <c>match.score_exceeds_best_of</c>.</param>
/// <param name="Message">Текст на нейтральном языке — для логов и отладки.</param>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public bool IsNone => Code.Length == 0;
}
