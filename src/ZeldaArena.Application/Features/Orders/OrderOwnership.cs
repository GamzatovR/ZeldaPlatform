using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Orders;

/// <summary>
/// Заказ ищется по номеру и владельцу сразу, одним условием (docs/SPEC.md §15, IDOR):
/// чужой и несуществующий заказ неразличимы, и перебором номеров ничего не узнать.
/// </summary>
public static class OrderOwnership
{
    public static IQueryable<Order> OwnedBy(this IReadRepository<Order> orders, Guid userId, string number)
    {
        ArgumentNullException.ThrowIfNull(orders);
        ArgumentException.ThrowIfNullOrWhiteSpace(number);

        // Номер набирают руками из письма-чека: регистр и пробелы по краям не важны.
        var normalized = number.Trim().ToUpperInvariant();

        return orders.Query().Where(order => order.UserId == userId && order.Number == normalized);
    }
}