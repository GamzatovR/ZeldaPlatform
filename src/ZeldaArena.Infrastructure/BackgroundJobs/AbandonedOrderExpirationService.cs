using MediatR;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ZeldaArena.Application.Features.Orders.Commands.ExpireAbandonedOrders;

namespace ZeldaArena.Infrastructure.BackgroundJobs;

/// <summary>
/// Каждые пять минут отменяет брошенные заказы (docs/adr/ADR-0009): остаток списан
/// при оформлении, и заказ, который никто не оплатит, не должен держать товар.
/// Пять минут — на порядок меньше льготного срока после истечения кода, поэтому
/// брошенный заказ освобождает склад почти вовремя, а база не опрашивается зря.
/// </summary>
public sealed class AbandonedOrderExpirationService(
    IServiceScopeFactory scopeFactory,
    ILogger<AbandonedOrderExpirationService> logger)
    : PeriodicCommandService(scopeFactory, logger)
{
    protected override TimeSpan Interval => TimeSpan.FromMinutes(5);

    protected override string FailureMessage => "Не удалось отменить брошенные заказы.";

    protected override IBaseRequest CreateCommand() => new ExpireAbandonedOrdersCommand();
}