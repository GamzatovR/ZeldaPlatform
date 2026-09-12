namespace ZeldaArena.Domain.Constants;

/// <summary>Правила подтверждения оплаты.</summary>
public static class PaymentPolicy
{
    /// <summary>Шесть цифр — компромисс между удобством ввода и стойкостью к подбору.</summary>
    public const int CodeLength = 6;

    public static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);

    /// <summary>Пауза между отправками кода.</summary>
    public static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);
}