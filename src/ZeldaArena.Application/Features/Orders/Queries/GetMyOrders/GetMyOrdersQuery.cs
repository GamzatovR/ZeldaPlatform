using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Orders.Queries.GetMyOrders;

/// <summary>
/// История заказов покупателя (docs/SPEC.md §9.3, п. 15) — третий фильтруемый список
/// на общем механизме §10.2: <c>/orders?status=paid&amp;sort=placed_desc&amp;page=2</c>.
/// Владелец берётся из текущего запроса, а не из адреса.
/// </summary>
public sealed record GetMyOrdersQuery : FilterBase, IQuery<PagedResult<OrderListItemDto>>
{
    public OrderStatus? Status { get; init; }
}