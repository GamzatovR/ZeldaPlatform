using ZeldaArena.Domain.Constants;

namespace ZeldaArena.Web.Authorization;

/// <summary>
/// Политики доступа из docs/SPEC.md §8.1.
///
/// Все построены на ролях и claim'ах, потому что описывают должностные полномочия.
/// Платные функции сюда не попадают никогда: их даёт подписка, и проверяются они
/// через IEntitlementService и динамические политики Feature:{code} (§7.3, §20 пункт 2).
/// </summary>
public static class AuthorizationRegistration
{
    public static IServiceCollection AddPlatformAuthorization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAuthorizationBuilder()
            .AddPolicy(PolicyNames.AdminOnly, policy =>
                policy.RequireRole(RoleNames.Admin))

            .AddPolicy(PolicyNames.ModeratorOrAdmin, policy =>
                policy.RequireRole(RoleNames.Admin, RoleNames.Moderator))

            // Модератор ведёт справочники, администратор может всё.
            .AddPolicy(PolicyNames.CanManageCatalog, policy =>
                policy.RequireRole(RoleNames.Admin, RoleNames.Moderator))

            // Деньги — исключительно администратор: у модератора нет доступа
            // к тарифам, подписками и платежам (§8.1).
            .AddPolicy(PolicyNames.CanManageBilling, policy =>
                policy.RequireRole(RoleNames.Admin))

            .AddPolicy(PolicyNames.CanModerateComments, policy =>
                policy.RequireRole(RoleNames.Admin, RoleNames.Moderator))

            .AddPolicy(PolicyNames.CanViewAuditLog, policy =>
                policy.RequireRole(RoleNames.Admin))

            .AddPolicy(PolicyNames.EmailConfirmed, policy =>
                policy.RequireClaim(AppClaimTypes.EmailConfirmed, AppClaimTypes.True));

        return services;
    }
}