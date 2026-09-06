namespace ZeldaArena.Domain.Common.Exceptions;

/// <summary>
/// Попытка привести сущность в состояние, запрещённое её инвариантами:
/// счёт больше формата матча, отрицательная сумма, правка завершённого матча.
/// </summary>
public sealed class InvariantViolationException(string code, string message)
    : DomainException(code, message)
{
    public static void Throw(string code, string message) =>
        throw new InvariantViolationException(code, message);

    public static void ThrowIf(bool condition, string code, string message)
    {
        if (condition)
        {
            Throw(code, message);
        }
    }
}