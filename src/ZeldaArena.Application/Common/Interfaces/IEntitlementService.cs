using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.Application.Common.Interfaces;

/// <summary>
/// Единственный источник истины о правах на платные функции (docs/SPEC.md §7.3).
/// Сигнатура зафиксирована спецификацией. Любая проверка доступа к платной функции
/// идёт сюда — ни <c>User.IsInRole("Premium")</c>, ни сравнение тарифов не допускаются
/// (§20 пункт 2).
///
/// Реализация появляется в Фазе 4: активные подписки → объединение фич тарифов →
/// кэш с TTL 5 минут, сбрасываемый доменными событиями подписки и правкой тарифа
/// в админке.
/// </summary>
public interface IEntitlementService
{
    Task<bool> HasFeatureAsync(
        Guid userId,
        string featureCode,
        CancellationToken cancellationToken = default);

    Task<EntitlementSet> GetEntitlementsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Значение параметризованной фичи (EP-5), например лимит команд или процент скидки.
    /// </summary>
    Task<string?> GetFeatureValueAsync(
        Guid userId,
        string featureCode,
        CancellationToken cancellationToken = default);
}