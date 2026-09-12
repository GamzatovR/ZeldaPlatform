namespace ZeldaArena.Web.Authorization;

/// <summary>Имена политик авторизации.</summary>
public static class PolicyNames
{
    public const string AdminOnly = "AdminOnly";

    public const string ModeratorOrAdmin = "ModeratorOrAdmin";

    public const string CanManageCatalog = "CanManageCatalog";

    /// <summary>Тарифы, фичи, подписки, платежи — только администратор.</summary>
    public const string CanManageBilling = "CanManageBilling";

    public const string CanModerateComments = "CanModerateComments";

    public const string CanViewAuditLog = "CanViewAuditLog";

    public const string EmailConfirmed = "EmailConfirmed";
}