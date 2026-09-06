namespace ZeldaArena.Domain.Common;

/// <summary>
/// Исход операции без возвращаемого значения. Объявлен в Domain по docs/SPEC.md §5.3
/// и переиспользуется хендлерами Application, чтобы тип результата был один на всё решение.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess != error.IsNone)
        {
            throw new ArgumentException(
                "Успех не может нести ошибку, неудача — не может быть без ошибки.",
                nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}
