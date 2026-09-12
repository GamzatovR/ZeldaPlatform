using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Orders.Queries.GetMyOrders;

public sealed record GetMyOrdersQuery : FilterBase, IQuery<PagedResult<OrderListItemDto>>
{
    public OrderStatus? Status { get; init; }
}