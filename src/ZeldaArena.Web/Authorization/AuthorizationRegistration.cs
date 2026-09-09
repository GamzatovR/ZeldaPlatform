using Microsoft.AspNetCore.Authorization;

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

        // Политики платных функций не перечисляются: FeaturePolicyProvider собирает
        // Feature:{code} на лету, поэтому новая функция не требует правки этого файла
        // (docs/SPEC.md §7.3, EP-3). Всё остальное он отдаёт провайдеру по умолчанию.
        services.AddSingleton<IAuthorizationPolicyProvider, FeaturePolicyProvider>();
        services.AddScoped<IAuthorizationHandler, FeatureAuthorizationHandler>();

        // Отказ из-за отсутствующей подписки уводит на тарифы, а не отвечает 403 (§7.3).
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, FeatureAccessDeniedHandler>();

        return services;
    }
}