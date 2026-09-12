namespace ZeldaArena.Domain.Constants;

/// <summary>Требования к паролю.</summary>
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