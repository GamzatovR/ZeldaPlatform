using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Application.Features.Admin.Orders.Queries.GetOrdersForAdmin;

/// <summary>Таблица заказов — <c>/admin/orders</c>.</summary>
public sealed record GetOrdersForAdminQuery : FilterBase, IQuery<PagedResult<AdminOrderRowDto>>
{
    public string? Search { get; init; }

    public OrderStatus? Status { get; init; }
}