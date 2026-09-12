using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Shop;
using ZeldaArena.Application.Common.Rules;
using ZeldaArena.Application.Features.Orders;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Admin.Orders.Commands.ChangeOrderStatus;

public sealed class ChangeOrderStatusCommandHandler(
    IReadRepository<Order> ordersForRead,
    IRepository<Order> orders,
    IRepository<Payment> payments,
    IReadRepository<Payment> paymentsForRead,
    OrderCancellation cancellation,
    IQueryExecutor queryExecutor,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeOrderStatusCommand, Result>
{
    public async Task<Result> Handle(ChangeOrderStatusCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var number = request.Number.Trim();

        var id = await queryExecutor.FirstOrDefaultAsync(
            ordersForRead.Query().Where(order => order.Number == number).Select(order => order.Id),
            cancellationToken);

        if (id == Guid.Empty)
        {
            return Result.Failure(ShopErrors.OrderNotFound);
        }

        var order = await orders.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            return Result.Failure(ShopErrors.OrderNotFound);
        }

        var wasPaid = order.Status == OrderStatus.Paid;

        var result = request.Transition == OrderTransition.Cancel
            ? await CancelAsync(order, wasPaid, cancellationToken)
            : DomainRules.Apply(request.Transition == OrderTransition.Ship ? order.Ship : order.Complete);

        if (result.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private async Task<Result> CancelAsync(Order order, bool wasPaid, CancellationToken cancellationToken)
    {
        Result canceled;

        try
        {
            await cancellation.CancelAsync(order, returnItemsToCart: false, cancellationToken);
            canceled = Result.Success();
        }
        catch (Domain.Common.Exceptions.DomainException exception)
        {
            return Result.Failure(new Error(exception.Code, exception.Message));
        }

        if (!wasPaid)
        {
            return canceled;
        }

        var paid = await queryExecutor.ToListAsync(
            paymentsForRead.Query()
                .Where(payment => payment.OrderId == order.Id && payment.Status == PaymentStatus.Succeeded)
                .Select(payment => payment.Id),
            cancellationToken);

        foreach (var payment in await payments.GetByIdsAsync(paid, cancellationToken))
        {
            payment.Refund(clock.UtcNow);
        }

        return canceled;
    }
}