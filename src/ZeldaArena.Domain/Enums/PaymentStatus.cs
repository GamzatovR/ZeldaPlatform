namespace ZeldaArena.Domain.Enums;

public enum PaymentStatus
{
    Pending = 0,
    Succeeded = 1,
    Failed = 2,
    Canceled = 3,

    /// <summary>Деньги возвращены покупателю.</summary>
    Refunded = 4,
}