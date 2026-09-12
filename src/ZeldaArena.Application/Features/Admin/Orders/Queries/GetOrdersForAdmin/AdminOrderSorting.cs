using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Admin.Orders.Queries.GetOrdersForAdmin;

public static class AdminOrderSorting
{
    public const string PlacedDescending = "placed_desc";
    public const string PlacedAscending = "placed_asc";
    public const string TotalDescending = "total_desc";
    public const string TotalAscending = "total_asc";

    public static readonly SortMap<Order> Map = new SortMap<Order>()
        .Add(PlacedDescending, query => query.OrderByDescending(order => order.PlacedAt), isDefault: true)
        .Add(PlacedAscending, query => query.OrderBy(order => order.PlacedAt))
        .Add(TotalDescending, query => query.OrderByDescending(order => order.Total))
        .Add(TotalAscending, query => query.OrderBy(order => order.Total));
}