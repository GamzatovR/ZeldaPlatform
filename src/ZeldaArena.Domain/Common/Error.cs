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

    /// <summary>
    /// Значения, подставляемые в перевод. Сообщения обязаны быть конкретными
    /// (docs/SPEC.md §15: «На складе осталось 3 шт.»), а число знает только сценарий —
    /// ресурс хранит шаблон, сценарий передаёт аргументы.
    /// </summary>
    public IReadOnlyList<object> Arguments { get; init; } = [];

    public bool IsNone => Code.Length == 0;

    public Error WithArguments(params object[] arguments) => this with { Arguments = arguments };

    /// <summary>
    /// Сравнение по содержимому аргументов, а не по ссылке на список: две ошибки
    /// «осталось 3 шт.» — одна и та же ошибка.
    /// </summary>
    public bool Equals(Error? other) =>
        other is not null
        && string.Equals(Code, other.Code, StringComparison.Ordinal)
        && string.Equals(Message, other.Message, StringComparison.Ordinal)
        && Arguments.SequenceEqual(other.Arguments);

    public override int GetHashCode() => HashCode.Combine(Code, Message);
}