namespace ZeldaArena.Domain.Enums;

/// <summary>Исход проверки кода подтверждения.</summary>
public enum PaymentConfirmationResult
{
    Succeeded = 0,
    WrongCode = 1,
    Expired = 2,
    NoAttemptsLeft = 3,
    AlreadyProcessed = 4,
}