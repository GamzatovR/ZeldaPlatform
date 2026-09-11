using MediatR;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ZeldaArena.Application.Features.Subscriptions.Commands.ExpireDueSubscriptions;

namespace ZeldaArena.Infrastructure.BackgroundJobs;

/// <summary>
/// Раз в час помечает истёкшие подписки (docs/SPEC.md §7.5, п. 4). Часовой точности
/// этой задаче хватает с запасом.
/// </summary>
public sealed class SubscriptionExpirationService(
    IServiceScopeFactory scopeFactory,
    ILogger<SubscriptionExpirationService> logger)
    : PeriodicCommandService(scopeFactory, logger)
{
    protected override TimeSpan Interval => TimeSpan.FromHours(1);

    protected override string FailureMessage => "Не удалось обработать истёкшие подписки.";

    protected override IBaseRequest CreateCommand() => new ExpireDueSubscriptionsCommand();
}