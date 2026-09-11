using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Orders.Queries.GetMyOrders;

/// <summary>Разрешённые сортировки истории заказов — whitelist из docs/SPEC.md §15.</summary>
public static class OrderSorting
{
    public const string PlacedDescending = "placed_desc";
    public const string PlacedAscending = "placed_asc";
    public const string TotalDescending = "total_desc";
    public const string TotalAscending = "total_asc";

    public static readonly SortMap<Order> Map = new SortMap<Order>()
        .Add(PlacedDescending, query => query.OrderByDescending(order => order.PlacedAt), isDefault: true)
        .Add(PlacedAscending, query => query.OrderBy(order => order.PlacedAt))
        .Add(TotalDescending, query => query.OrderByDescending(order => order.Total).ThenByDescending(order => order.PlacedAt))
        .Add(TotalAscending, query => query.OrderBy(order => order.Total).ThenByDescending(order => order.PlacedAt));
}