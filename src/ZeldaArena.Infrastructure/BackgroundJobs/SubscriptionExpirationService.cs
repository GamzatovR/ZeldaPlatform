using MediatR;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ZeldaArena.Application.Features.Subscriptions.Commands.ExpireDueSubscriptions;

namespace ZeldaArena.Infrastructure.BackgroundJobs;

public sealed class SubscriptionExpirationService(
    IServiceScopeFactory scopeFactory,
    ILogger<SubscriptionExpirationService> logger)
    : PeriodicCommandService(scopeFactory, logger)
{
    protected override TimeSpan Interval => TimeSpan.FromHours(1);

    protected override string FailureMessage => "Не удалось обработать истёкшие подписки.";

    protected override IBaseRequest CreateCommand() => new ExpireDueSubscriptionsCommand();
}