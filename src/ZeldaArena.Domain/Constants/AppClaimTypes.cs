namespace ZeldaArena.Domain.Constants;

/// <summary>Собственные типы claim'ов, которые кладутся в cookie при входе.</summary>
public static class AppClaimTypes
{
    public const string EmailConfirmed = "zelda:email_confirmed";

    public const string TwoFactorEnabled = "zelda:two_factor_enabled";

    public const string DisplayName = "zelda:display_name";

    /// <summary>Значение булева claim'а: сравнение идёт по нему, а не по «true» в любом регистре.</summary>
    public const string True = "true";
}