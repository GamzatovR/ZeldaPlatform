using Microsoft.EntityFrameworkCore;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Infrastructure.Persistence.Ef;

namespace ZeldaArena.Infrastructure.Identity;

/// <summary>
/// Единственный источник истины о правах на платные функции (docs/SPEC.md §7.3).
///
/// Здесь только загрузка и кэш. Правила отбора — какая подписка действует, как
/// объединяются фичи двух тарифов, чей параметр побеждает — живут в
/// <see cref="EntitlementResolver"/> в Application, где их покрывают unit-тесты:
/// ZeldaArena.UnitTests по §5.1 на Infrastructure не ссылается.
/// </summary>
public sealed class EntitlementService(
    AppDbContext context,
    EntitlementCache cache,
    IDateTimeProvider clock)
    : IEntitlementService
{
    public async Task<bool> HasFeatureAsync(
        Guid userId,
        string featureCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureCode);

        var entitlements = await GetEntitlementsAsync(userId, cancellationToken).ConfigureAwait(false);

        return entitlements.Has(featureCode);
    }

    public Task<EntitlementSet> GetEntitlementsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Аноним платных функций не имеет, и спрашивать об этом базу незачем.
        return userId == Guid.Empty
            ? Task.FromResult(EntitlementSet.Empty)
            : cache.GetOrCreateAsync(
                userId,
                () => LoadAsync(userId, cancellationToken),
                cancellationToken);
    }

    public async Task<string?> GetFeatureValueAsync(
        Guid userId,
        string featureCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureCode);

        var entitlements = await GetEntitlementsAsync(userId, cancellationToken).ConfigureAwait(false);

        return entitlements.ValueOf(featureCode);
    }

    /// <summary>
    /// Три коротких запроса вместо одного соединения: подписок у пользователя единицы,
    /// тарифов и фич во всей базе — десятки, а результат живёт пять минут. Отбор по
    /// сроку и статусу нарочно не переносится в SQL — он объявлен в домене
    /// (<c>Subscription.IsActiveAt</c>), и второй его копии в виде where-условия быть
    /// не должно.
    /// </summary>
    private async Task<EntitlementSet> LoadAsync(Guid userId, CancellationToken cancellationToken)
    {
        var subscriptions = await context.Subscriptions
            .AsNoTracking()
            .Where(subscription => subscription.UserId == userId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (subscriptions.Count == 0)
        {
            return EntitlementSet.Empty;
        }

        var planIds = subscriptions
            .Select(subscription => subscription.PlanId)
            .Distinct()
            .ToArray();

        var plans = await context.Plans
            .AsNoTracking()
            .Include(plan => plan.PlanFeatures)
            .Where(plan => planIds.Contains(plan.Id))
            .ToDictionaryAsync(plan => plan.Id, cancellationToken)
            .ConfigureAwait(false);

        var features = await context.Features
            .AsNoTracking()
            .ToDictionaryAsync(feature => feature.Id, cancellationToken)
            .ConfigureAwait(false);

        return EntitlementResolver.Resolve(subscriptions, plans, features, clock.UtcNow);
    }
}