namespace ZeldaArena.Domain.Constants;

/// <summary>Имена ролей.</summary>
public static class RoleNames
{
    public const string Admin = "Admin";

    public const string Moderator = "Moderator";

    public const string User = "User";

    /// <summary>Техническая роль-бейдж. Для проверки доступа не используется никогда.</summary>
    public const string Premium = "Premium";

    /// <summary>Роли, создаваемые сидом.</summary>
    public static IReadOnlyList<string> All { get; } = [Admin, Moderator, User, Premium];
}