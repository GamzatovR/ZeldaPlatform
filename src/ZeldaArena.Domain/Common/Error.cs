namespace ZeldaArena.Domain.Common;

/// <summary>Ошибка сценария.</summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>Значения, подставляемые в перевод.</summary>
    public IReadOnlyList<object> Arguments { get; init; } = [];

    public bool IsNone => Code.Length == 0;

    public Error WithArguments(params object[] arguments) => this with { Arguments = arguments };

    /// <summary>Сравнение по содержимому аргументов, а не по ссылке на список.</summary>
    public bool Equals(Error? other) =>
        other is not null
        && string.Equals(Code, other.Code, StringComparison.Ordinal)
        && string.Equals(Message, other.Message, StringComparison.Ordinal)
        && Arguments.SequenceEqual(other.Arguments);

    public override int GetHashCode() => HashCode.Combine(Code, Message);
}