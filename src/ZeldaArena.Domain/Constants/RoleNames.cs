namespace ZeldaArena.Domain.Constants;

/// <summary>
/// Имена ролей. Три содержательные роли из docs/SPEC.md §8.1 плюс техническая Premium.
///
/// Premium стоит особняком: она выдаётся и снимается обработчиками событий подписки
/// и годится только для отображения — бейдж у ника, метка в чате. Проверять по ней
/// доступ запрещено, права даёт исключительно IEntitlementService (§7.4, §20 пункт 2).
/// </summary>
public static class RoleNames
{
    public const string Admin = "Admin";

    public const string Moderator = "Moderator";

    public const string User = "User";

    /// <summary>Техническая роль-бейдж. Для проверки доступа не используется никогда.</summary>
    public const string Premium = "Premium";

    /// <summary>Роли, создаваемые сидом (docs/SPEC.md §6).</summary>
    public static IReadOnlyList<string> All { get; } = [Admin, Moderator, User, Premium];
}