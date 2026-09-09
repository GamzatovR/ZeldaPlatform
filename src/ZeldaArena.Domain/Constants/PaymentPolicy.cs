namespace ZeldaArena.Domain.Constants;

/// <summary>
/// Правила подтверждения оплаты из docs/SPEC.md §7.6.
///
/// Живут в Domain по той же причине, что и <see cref="PasswordPolicy"/>: одни и те же
/// числа нужны трём слоям. Хендлер в Application считает по ним срок жизни кода
/// и паузу между отправками, валидатор проверяет длину введённого кода, а генератор
/// в Infrastructure выпускает код нужной длины. Разъехавшись, они дали бы форму,
/// которая принимает код, не совпадающий с выданным.
///
/// Число попыток объявлено в самой сущности (<c>Payment.MaxAttempts</c>): его
/// расходует и проверяет только она.
/// </summary>
public static class PaymentPolicy
{
    /// <summary>Шесть цифр — компромисс между удобством ввода и стойкостью к подбору.</summary>
    public const int CodeLength = 6;

    public static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Пауза между отправками кода. Ограничение уровня сценария, а не только
    /// rate limiting: последний считает запросы с адреса, а этот запрет действует
    /// на платёж, поэтому его не обойти вторым окном браузера.
    /// </summary>
    public static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);
}