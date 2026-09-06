using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Domain.Shop;

/// <summary>
/// Корзина гостя или пользователя. Гостевая опознаётся подписанной кукой AnonymousId,
/// которую выдаёт CartCookieMiddleware; при входе гостевая корзина сливается
/// с пользовательской (docs/SPEC.md §14.1).
/// </summary>
public class Cart : BaseEntity, IAuditableEntity
{
    private readonly List<CartItem> _items = [];

    private Cart()
    {
    }

    public Guid? UserId { get; private set; }

    public Guid? AnonymousId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public bool IsEmpty => _items.Count == 0;

    public int TotalQuantity => _items.Sum(item => item.Quantity);

    /// <summary>
    /// Итог считается здесь и только здесь: суммы, приходящие с клиента,
    /// не принимаются на веру (docs/SPEC.md §20, п. 7).
    /// </summary>
    public Money Subtotal => new(_items.Sum(item => item.LineTotal), Money.DefaultCurrency);

    public static Cart ForUser(Guid userId)
    {
        InvariantViolationException.ThrowIf(
            userId == Guid.Empty,
            "cart.owner_required",
            "Корзине пользователя нужен идентификатор пользователя.");

        return new Cart { UserId = userId };
    }

    public static Cart ForGuest(Guid anonymousId)
    {
        InvariantViolationException.ThrowIf(
            anonymousId == Guid.Empty,
            "cart.owner_required",
            "Гостевой корзине нужен анонимный идентификатор.");

        return new Cart { AnonymousId = anonymousId };
    }

    /// <summary>
    /// Добавляет товар. Цена берётся у товара, а не из запроса, количество проверяется
    /// по остатку. Повторное добавление увеличивает количество той же позиции.
    /// </summary>
    public CartItem AddItem(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        EnsureSellable(product, quantity);

        var existing = _items.SingleOrDefault(item => item.ProductId == product.Id);
        if (existing is not null)
        {
            var increased = existing.Quantity + quantity;
            EnsureStock(product, increased);
            existing.SetQuantity(increased);
            existing.RefreshPrice(product.Price.Amount);
            return existing;
        }

        var added = CartItem.Create(Id, product.Id, quantity, product.Price.Amount);
        _items.Add(added);
        return added;
    }

    public void ChangeQuantity(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        EnsureSellable(product, quantity);

        var item = _items.SingleOrDefault(entry => entry.ProductId == product.Id);

        InvariantViolationException.ThrowIf(
            item is null,
            "cart.item_not_found",
            "Такого товара в корзине нет.");

        item!.SetQuantity(quantity);
        item.RefreshPrice(product.Price.Amount);
    }

    public void RemoveItem(Guid productId)
    {
        var item = _items.SingleOrDefault(entry => entry.ProductId == productId);

        InvariantViolationException.ThrowIf(
            item is null,
            "cart.item_not_found",
            "Такого товара в корзине нет.");

        _items.Remove(item!);
    }

    public void Clear() => _items.Clear();

    /// <summary>
    /// Слияние гостевой корзины с пользовательской при входе: количества складываются,
    /// но не выше остатка на складе. Товары, которых уже нет в наличии, пропускаются.
    /// </summary>
    public void MergeFrom(Cart guestCart, IReadOnlyDictionary<Guid, Product> products)
    {
        ArgumentNullException.ThrowIfNull(guestCart);
        ArgumentNullException.ThrowIfNull(products);

        foreach (var guestItem in guestCart.Items)
        {
            if (!products.TryGetValue(guestItem.ProductId, out var product) || !product.IsActive)
            {
                continue;
            }

            var existing = _items.SingleOrDefault(item => item.ProductId == guestItem.ProductId);
            var wanted = (existing?.Quantity ?? 0) + guestItem.Quantity;
            var allowed = Math.Min(wanted, product.StockQuantity);

            if (allowed < 1)
            {
                continue;
            }

            if (existing is null)
            {
                _items.Add(CartItem.Create(Id, product.Id, allowed, product.Price.Amount));
            }
            else
            {
                existing.SetQuantity(allowed);
                existing.RefreshPrice(product.Price.Amount);
            }
        }
    }

    private static void EnsureSellable(Product product, int quantity)
    {
        InvariantViolationException.ThrowIf(
            !product.IsActive,
            "cart.product_unavailable",
            "Товар снят с продажи.");

        InvariantViolationException.ThrowIf(
            !string.Equals(product.Price.Currency, Money.DefaultCurrency, StringComparison.Ordinal),
            "cart.currency_mismatch",
            $"Магазин принимает оплату только в {Money.DefaultCurrency}.");

        InvariantViolationException.ThrowIf(
            quantity < 1,
            "cart.invalid_quantity",
            $"Количество товара начинается с единицы, получено {quantity}.");

        EnsureStock(product, quantity);
    }

    private static void EnsureStock(Product product, int quantity) =>
        InvariantViolationException.ThrowIf(
            quantity > product.StockQuantity,
            "cart.insufficient_stock",
            $"На складе осталось {product.StockQuantity} шт.");
}
