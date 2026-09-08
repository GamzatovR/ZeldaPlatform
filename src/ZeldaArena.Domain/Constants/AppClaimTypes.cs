namespace ZeldaArena.Domain.Constants;

/// <summary>
/// Собственные типы claim'ов, которые кладутся в cookie при входе.
///
/// Нужны, чтобы политики авторизации в Web могли спрашивать о свойствах учётной записи,
/// не обращаясь к UserManager: тот живёт в Infrastructure, а правило 3 docs/SPEC.md §5.2
/// запрещает его типы в слое представления.
///
/// Значения обновляются при перевыпуске cookie. Все сценарии, меняющие эти свойства,
/// вызывают RefreshSignIn — иначе claim отстал бы от базы до следующего входа.
/// </summary>
public static class AppClaimTypes
{
    public const string EmailConfirmed = "zelda:email_confirmed";

    public const string TwoFactorEnabled = "zelda:two_factor_enabled";

    public const string DisplayName = "zelda:display_name";

    /// <summary>Значение булева claim'а: сравнение идёт по нему, а не по «true» в любом регистре.</summary>
    public const string True = "true";
}