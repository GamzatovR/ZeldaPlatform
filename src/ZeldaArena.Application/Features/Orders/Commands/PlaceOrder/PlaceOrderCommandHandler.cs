using MediatR;

using ZeldaArena.Application.Common.Exceptions;
using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Common.Models.Identity;
using ZeldaArena.Application.Common.Models.Shop;
using ZeldaArena.Application.Features.Carts;
using ZeldaArena.Application.Features.Payments;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Shop;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Application.Features.Orders.Commands.PlaceOrder;

/// <summary>
/// Корзина → заказ в ожидании оплаты → платёж с кодом на почту, одной транзакцией
/// (docs/SPEC.md §7.6, §9.3 п. 14; решения — docs/adr/ADR-0009):
///
/// <list type="number">
///   <item>цены и названия берутся у товаров на сервере и ложатся в заказ снапшотом (§6);</item>
///   <item>карта проверяется до любых изменений — отклонённая карта не оставляет заказа;</item>
///   <item>остаток списывается сразу: дошедший до ввода кода покупатель гарантированно
///   получит товар, а провал оплаты вернёт остаток обратно (<see cref="OrderCancellation"/>);</item>
///   <item>корзина очищается — её содержимое теперь в заказе.</item>
/// </list>
///
/// Два покупателя последней единицы: остаток защищён токеном конкурентности, второй
/// получит понятный отказ, а не ошибку сервера (§15).
/// </summary>
public sealed class PlaceOrderCommandHandler(
    ICurrentUserService currentUser,
    CartLocator carts,
    IRepository<Product> products,
    IRepository<Order> orders,
    OrderNumberGenerator numbers,
    PaymentInitiator initiator,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock)
    : IRequestHandler<PlaceOrderCommand, Result<StartPaymentResult>>
{
    public async Task<Result<StartPaymentResult>> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (currentUser.UserId is not { } userId)
        {
            return Result.Failure<StartPaymentResult>(AccountErrors.UserNotFound);
        }

        // Повторная отправка той же формы возвращает уже заведённый платёж: корзина
        // к этому моменту пуста, и второй заказ из неё не собрался бы всё равно.
        var existing = await initiator.FindExistingAsync(userId, request, cancellationToken).ConfigureAwait(false);

        if (existing is not null)
        {
            return Result.Success(existing);
        }

        var cart = await carts.FindAsync(CartOwner.ForUser(userId), cancellationToken).ConfigureAwait(false);

        if (cart is null || cart.IsEmpty)
        {
            return Result.Failure<StartPaymentResult>(ShopErrors.CartEmpty);
        }

        var productIds = cart.Items.Select(item => item.ProductId).ToArray();
        var loaded = (await products.GetByIdsAsync(productIds, cancellationToken).ConfigureAwait(false))
            .ToDictionary(product => product.Id);

        var lines = new List<(Product Product, int Quantity)>(cart.Items.Count);

        foreach (var item in cart.Items)
        {
            var product = loaded.GetValueOrDefault(item.ProductId);

            // Остаток и статус могли измениться, пока товар лежал в корзине. Страница
            // корзины покажет, какая именно позиция мешает, — туда и отправит контроллер.
            if (CartStockCheck.Check(product, item.Quantity) is not null)
            {
                return Result.Failure<StartPaymentResult>(ShopErrors.CartHasProblems);
            }

            lines.Add((product!, item.Quantity));
        }

        var total = new Money(lines.Sum(line => line.Product.Price.Amount * line.Quantity), Money.DefaultCurrency);

        var authorization = await initiator.AuthorizeAsync(request, total, cancellationToken).ConfigureAwait(false);

        if (authorization.IsFailure)
        {
            return Result.Failure<StartPaymentResult>(authorization.Error);
        }

        var now = clock.UtcNow;
        var number = await numbers.NextAsync(now, cancellationToken).ConfigureAwait(false);

        var order = Order.Place(
            userId,
            number,
            new ShippingAddress(request.Recipient, request.Phone, request.Country, request.City, request.Street, request.PostalCode),
            lines.Select(line => new OrderLine(line.Product.Id, line.Product.Name, line.Product.Price.Amount, line.Quantity)),
            now);

        await orders.AddAsync(order, cancellationToken).ConfigureAwait(false);

        foreach (var (product, quantity) in lines)
        {
            product.DecreaseStock(quantity);
        }

        cart.Clear();

        var initiated = await initiator
            .OpenAsync(userId, PaymentPurpose.Order, order.TotalMoney, authorization.Value, request, cancellationToken, orderId: order.Id)
            .ConfigureAwait(false);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (ConcurrencyConflictException)
        {
            return Result.Failure<StartPaymentResult>(ShopErrors.StockChanged);
        }

        await initiator.SendCodeAsync(initiated, currentUser.UserName, cancellationToken).ConfigureAwait(false);

        return Result.Success(initiated.ToResult());
    }
}