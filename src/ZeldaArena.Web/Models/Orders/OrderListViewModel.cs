using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Features.Orders.Queries.GetMyOrders;

namespace ZeldaArena.Web.Models.Orders;

/// <summary>История заказов: фильтр из адреса и страница результата.</summary>
public sealed class OrderListViewModel
{
    public required GetMyOrdersQuery Filter { get; init; }

    public required PagedResult<OrderListItemDto> Result { get; init; }
}