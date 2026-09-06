namespace ZeldaArena.Domain.Enums;

/// <summary>
/// Исход проверки кода подтверждения. Неверный код — это ожидаемое поведение
/// пользователя, а не ошибка программы, поэтому возвращается значением,
/// а не исключением: форме нужно показать, сколько попыток осталось (docs/SPEC.md §7.6).
/// </summary>
public enum PaymentConfirmationResult
{
    Succeeded = 0,
    WrongCode = 1,
    Expired = 2,
    NoAttemptsLeft = 3,
    AlreadyProcessed = 4,
}
