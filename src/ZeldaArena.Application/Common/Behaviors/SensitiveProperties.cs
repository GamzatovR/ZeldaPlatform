namespace ZeldaArena.Application.Common.Behaviors;

/// <summary>Свойства, значения которых не должны попадать ни в логи, ни в аудит.</summary>
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
        "authenticatorkey",
        "authenticatoruri",
        "sharedkey",
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