namespace ZeldaArena.Domain.Common.Exceptions;

/// <summary>
/// Базовое исключение предметной области. <see cref="Code"/> — ключ ресурса:
/// GlobalExceptionHandlingMiddleware (docs/SPEC.md §14.1) переводит его в
/// локализованное сообщение и отвечает 400 или 409, не раскрывая стек.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string code, string message)
        : base(message) => Code = code;

    protected DomainException(string code, string message, Exception innerException)
        : base(message, innerException) => Code = code;

    public string Code { get; }
}