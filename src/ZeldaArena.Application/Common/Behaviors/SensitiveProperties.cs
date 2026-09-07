namespace ZeldaArena.Application.Common.Behaviors;

/// <summary>
/// Свойства, значения которых не должны попадать ни в логи, ни в аудит
/// (docs/SPEC.md §13, §7.6): пароли, токены, recovery-коды, номер карты, CVV
/// и код подтверждения оплаты.
///
/// Совпадение ищется по фрагменту имени, а не по точному списку: команда назовёт поле
/// <c>NewPassword</c>, <c>ConfirmationCode</c> или <c>CardNumberInput</c>, и все три
/// обязаны быть закрыты. Фрагмент <c>code</c> сам по себе не запрещён намеренно —
/// иначе под маску попали бы коды фич и тарифов, которые в аудите как раз нужны.
/// </summary>
public static class SensitiveProperties
{
    private static readonly string[] Fragments =
    [
        "password",
        "secret",
        "token",
        "cvv",
        "cvc",
        "cardnumber",
        "confirmationcode",
        "recoverycode",
        "twofactorcode",
        "apikey",
        "connectionstring",
    ];

    public const string Mask = "***";

    public static bool IsSensitive(string propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        return Array.Exists(
            Fragments,
            fragment => propertyName.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }
}