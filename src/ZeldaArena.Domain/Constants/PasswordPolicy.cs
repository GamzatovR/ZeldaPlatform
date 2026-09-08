namespace ZeldaArena.Domain.Constants;

/// <summary>
/// Требования к паролю из docs/SPEC.md §8.2.
///
/// Живут в Domain, потому что нужны сразу двум слоям: FluentValidation в Application
/// проверяет их до хендлера и даёт конкретное сообщение (§15), а Identity
/// в Infrastructure настраивает по ним PasswordOptions. Разъехавшись, эти два места
/// дали бы пароль, который форма принимает, а Identity отвергает.
/// </summary>
public static class PasswordPolicy
{
    public const int MinimumLength = 10;

    /// <summary>Верхняя граница — защита от очень длинного ввода, а не требование к силе.</summary>
    public const int MaximumLength = 128;

    public const bool RequireDigit = true;

    public const bool RequireLowercase = true;

    public const bool RequireUppercase = true;

    public const bool RequireNonAlphanumeric = true;
}