using MediatR;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ZeldaArena.Application.Features.Orders.Commands.ExpireAbandonedOrders;

namespace ZeldaArena.Infrastructure.BackgroundJobs;

public sealed class AbandonedOrderExpirationService(
    IServiceScopeFactory scopeFactory,
    ILogger<AbandonedOrderExpirationService> logger)
    : PeriodicCommandService(scopeFactory, logger)
{
    protected override TimeSpan Interval => TimeSpan.FromMinutes(5);

    protected override string FailureMessage => "Не удалось отменить брошенные заказы.";

    protected override IBaseRequest CreateCommand() => new ExpireAbandonedOrdersCommand();
}