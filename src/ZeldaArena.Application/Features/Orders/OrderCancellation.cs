using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Features.Carts;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Orders;

public sealed class OrderCancellation(
    IRepository<Product> products,
    IRepository<Payment> payments,
    IReadRepository<Payment> paymentsForRead,
    CartLocator carts,
    IQueryExecutor queryExecutor,
    IDateTimeProvider clock)
{
    /// <summary>Причина, с которой закрывается платёж отменённого заказа.</summary>
    public const string PaymentFailureReason = "order.canceled";

    public async Task CancelAsync(Order order, bool returnItemsToCart, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(order);

        order.Cancel(clock.UtcNow);

        // Товары грузятся одним запросом, а не по одному на позицию.
        var productIds = order.Items.Select(item => item.ProductId).Distinct().ToArray();
        var loaded = (await products.GetByIdsAsync(productIds, cancellationToken).ConfigureAwait(false))
            .ToDictionary(product => product.Id);

        foreach (var item in order.Items)
        {
            // Товар не удаляется (OrderItems → Products — Restrict), но проверка
            // дешевле, чем исключение посреди отмены.
            if (loaded.TryGetValue(item.ProductId, out var product))
            {
                product.IncreaseStock(item.Quantity);
            }
        }

        if (returnItemsToCart)
        {
            await ReturnToCartAsync(order, loaded, cancellationToken).ConfigureAwait(false);
        }

        await ClosePendingPaymentsAsync(order, cancellationToken).ConfigureAwait(false);
    }

    private async Task ReturnToCartAsync(
        Order order,
        IReadOnlyDictionary<Guid, Product> loaded,
        CancellationToken cancellationToken)
    {
        var cart = await carts.GetOrCreateAsync(CartOwner.ForUser(order.UserId), cancellationToken).ConfigureAwait(false);

        foreach (var item in order.Items)
        {
            if (!loaded.TryGetValue(item.ProductId, out var product) || !product.IsActive)
            {
                continue;
            }

            var inCart = cart.Items.SingleOrDefault(line => line.ProductId == product.Id)?.Quantity ?? 0;
            var toAdd = Math.Min(inCart + item.Quantity, product.StockQuantity) - inCart;

            if (toAdd > 0)
            {
                cart.AddItem(product, toAdd);
            }
        }
    }

    private async Task ClosePendingPaymentsAsync(Order order, CancellationToken cancellationToken)
    {
        var pendingIds = await queryExecutor
            .ToListAsync(
                paymentsForRead.Query()
                    .Where(payment => payment.OrderId == order.Id && payment.Status == PaymentStatus.Pending)
                    .Select(payment => payment.Id),
                cancellationToken)
            .ConfigureAwait(false);

        if (pendingIds.Count == 0)
        {
            return;
        }

        foreach (var payment in await payments.GetByIdsAsync([.. pendingIds], cancellationToken).ConfigureAwait(false))
        {
            // Платёж мог уже провалиться или быть отменён в этом же запросе — тогда
            // он не в ожидании и повторно не трогается.
            if (payment.IsPending)
            {
                payment.Fail(PaymentFailureReason);
            }
        }
    }
}