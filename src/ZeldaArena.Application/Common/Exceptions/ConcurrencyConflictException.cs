namespace ZeldaArena.Application.Common.Exceptions;

/// <summary>
/// Запись изменили между чтением и сохранением (docs/SPEC.md §15: RowVersion +
/// обработка конфликта). Объявлено здесь, а не взято из EF Core, потому что
/// Application о EF Core не знает (§5.2, правило 2): реализация <c>IUnitOfWork</c>
/// переводит своё исключение в это, и сценарий может ответить на гонку понятным
/// сообщением, а не ошибкой сервера.
///
/// Типичные случаи — двое покупают последнюю единицу товара, два модератора правят
/// счёт одного матча.
/// </summary>
public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException()
        : base("Запись изменена параллельным запросом.")
    {
    }

    public ConcurrencyConflictException(string message)
        : base(message)
    {
    }

    public ConcurrencyConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}