using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;

using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.Web.Authorization;

/// <summary>
/// Проверяет платную функцию через <see cref="IEntitlementService"/> — единственный
/// допустимый источник истины о правах (docs/SPEC.md §7.3, §20 пункт 2).
///
/// Порт объявлен в Application, поэтому правило 3 §5.2 не нарушено: типов
/// Infrastructure здесь нет, реализацию подставляет composition root.
/// </summary>
public sealed class FeatureAuthorizationHandler(IEntitlementService entitlements)
    : AuthorizationHandler<FeatureRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        FeatureRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requirement);

        // Анониму предлагать подписку рано: сначала вход. Требование остаётся
        // невыполненным, и middleware уводит на страницу входа с returnUrl.
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        if (!Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return;
        }

        if (await entitlements.HasFeatureAsync(userId, requirement.FeatureCode).ConfigureAwait(false))
        {
            context.Succeed(requirement);
        }
    }
}