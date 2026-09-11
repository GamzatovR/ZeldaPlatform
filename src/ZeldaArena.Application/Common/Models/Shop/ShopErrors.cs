using ZeldaArena.Domain.Common;

namespace ZeldaArena.Application.Common.Models.Shop;

/// <summary>
/// Ошибки сценариев магазина. Коды — ключи ресурсов (docs/SPEC.md §9.5); там, где они
/// совпадают с кодами домена (<c>cart.insufficient_stock</c>), это намеренно: у одного
/// исхода один ключ, независимо от того, кто его обнаружил.
///
/// Сценарии проверяют эти условия сами, до вызова сущности: инвариант домена — последний
/// рубеж и отвечает исключением, а пользователь должен получить сообщение на форме.
/// </summary>
public static class ShopErrors
{
    public static readonly Error ProductNotFound =
        new("shop.product_not_found", "Товар не найден.");

    public static readonly Error ProductUnavailable =
        new("cart.product_unavailable", "Товар снят с продажи.");

    public static readonly Error OutOfStock =
        new("cart.out_of_stock", "Товара нет в наличии.");

    /// <summary>Шаблон «На складе осталось {0} шт.» — число подставляет сценарий.</summary>
    public static Error InsufficientStock(int available) =>
        new Error("cart.insufficient_stock", $"На складе осталось {available} шт.").WithArguments(available);

    public static readonly Error ItemNotFound =
        new("cart.item_not_found", "Такого товара в корзине нет.");

    /// <summary>
    /// Ни входа, ни гостевой куки: браузер не принимает cookie, и корзину некуда
    /// привязать. Без этого отказа товары молча пропадали бы между запросами.
    /// </summary>
    public static readonly Error CartUnavailable =
        new("cart.unavailable", "Корзина недоступна: разрешите cookie в браузере.");

    public static readonly Error CartEmpty =
        new("cart.empty", "Корзина пуста.");

    /// <summary>Корзину нельзя оформить, пока в ней есть позиция, которую нельзя купить.</summary>
    public static readonly Error CartHasProblems =
        new("checkout.cart_has_problems", "В корзине есть товары, которые нельзя купить в таком количестве.");

    /// <summary>Остаток изменили между чтением и сохранением: последнюю единицу купил кто-то другой.</summary>
    public static readonly Error StockChanged =
        new("checkout.stock_changed", "Пока вы оформляли заказ, остаток на складе изменился. Проверьте корзину.");

    public static readonly Error OrderNotFound =
        new("order.not_found", "Заказ не найден.");

    public static readonly Error OrderNotCancelable =
        new("order.not_cancelable", "Отменить можно только неоплаченный заказ.");

    public static readonly Error OrderNotPayable =
        new("order.not_payable", "Этот заказ уже не ждёт оплаты.");
}