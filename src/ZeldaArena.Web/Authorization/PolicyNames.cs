namespace ZeldaArena.Web.Authorization;

/// <summary>
/// Имена политик авторизации (docs/SPEC.md §8.1). Константы, а не литералы:
/// опечатка в имени политики даёт не ошибку компиляции, а молча закрытую страницу.
///
/// Динамические политики вида <c>Feature:{code}</c> собираются на лету
/// в Фазе 4 через IAuthorizationPolicyProvider (§7.3) и в этот список не входят.
/// </summary>
public static class PolicyNames
{
    public const string AdminOnly = "AdminOnly";

    public const string ModeratorOrAdmin = "ModeratorOrAdmin";

    /// <summary>
    /// Киберспортивные справочники: турниры, матчи, команды, игроки. Магазин сюда
    /// не входит: в правах модератора по §8.1 его нет, а заказ связан с платежами,
    /// поэтому товары и заказы закрыты <see cref="AdminOnly"/> (docs/adr/ADR-0010).
    /// </summary>
    public const string CanManageCatalog = "CanManageCatalog";

    /// <summary>Тарифы, фичи, подписки, платежи — только администратор.</summary>
    public const string CanManageBilling = "CanManageBilling";

    public const string CanModerateComments = "CanModerateComments";

    public const string CanViewAuditLog = "CanViewAuditLog";

    /// <summary>
    /// Подтверждённая почта. Требуется перед покупкой подписки и оформлением заказа
    /// (§8.2); сами эти сценарии появятся в Фазах 4 и 7.
    /// </summary>
    public const string EmailConfirmed = "EmailConfirmed";
}