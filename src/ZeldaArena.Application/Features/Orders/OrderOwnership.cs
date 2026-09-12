using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Orders;

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