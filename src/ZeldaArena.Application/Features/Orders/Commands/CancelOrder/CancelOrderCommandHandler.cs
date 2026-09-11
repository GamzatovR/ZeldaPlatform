using MediatR;

using ZeldaArena.Application.Common.Exceptions;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Common.Models.Shop;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Orders.Commands.CancelOrder;

/// <summary>
/// Домен разрешает отменить и оплаченный заказ (<c>Order.Cancel</c>), но покупателю
/// это не дано: вернуть деньги за мнимую оплату нечем, и такой возврат — решение
/// магазина, а не кнопка в кабинете. Ограничение живёт здесь, в сценарии покупателя;
/// сценарий администратора в Фазе 9 его не наследует.
///
/// Позиции в корзину не возвращаются: покупатель сам решил, что заказ ему не нужен.
/// </summary>
public sealed class CancelOrderCommandHandler(
    ICurrentUserService currentUser,
    IReadRepository<Order> ordersForRead,
    IRepository<Order> orders,
    OrderCancellation cancellation,
    IQueryExecutor queryExecutor,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CancelOrderCommand, Result>
{
    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure(AccountErrors.UserNotFound);
        }

        var orderId = await queryExecutor
            .FirstOrDefaultAsync(
                ordersForRead.OwnedBy(userId, request.Number).Select(order => (Guid?)order.Id),
                cancellationToken)
            .ConfigureAwait(false);

        var order = orderId is null
            ? null
            : await orders.GetByIdAsync(orderId.Value, cancellationToken).ConfigureAwait(false);

        if (order is null)
        {
            return Result.Failure(ShopErrors.OrderNotFound);
        }

        if (order.Status != OrderStatus.Pending)
        {
            return Result.Failure(ShopErrors.OrderNotCancelable);
        }

        await cancellation.CancelAsync(order, returnItemsToCart: false, cancellationToken).ConfigureAwait(false);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (ConcurrencyConflictException)
        {
            // Заказ оплатили в соседней вкладке или остаток его товара изменило чужое
            // оформление: ничего не сохранено, страница заказа покажет актуальный статус.
            return Result.Failure(ShopErrors.OrderChangedConcurrently);
        }

        return Result.Success();
    }
}