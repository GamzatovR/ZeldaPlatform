using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Orders.Queries.GetMyOrders;

/// <summary>
/// Только свои заказы (§15, IDOR): условие на владельца стоит первым и не зависит
/// от фильтра. Число штук — <c>Sum()</c> внутри проекции: EF Core переводит его
/// в скалярный подзапрос, а не тянет позиции каждого заказа (§16).
/// </summary>
public sealed class GetMyOrdersQueryHandler(
    ICurrentUserService currentUser,
    IReadRepository<Order> orders,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetMyOrdersQuery, PagedResult<OrderListItemDto>>
{
    public Task<PagedResult<OrderListItemDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (currentUser.UserId is not { } userId)
        {
            return Task.FromResult(PagedResult<OrderListItemDto>.Empty(request.NormalizedPage, request.NormalizedPageSize));
        }

        var query = orders.Query().Where(order => order.UserId == userId);

        if (request.Status is { } status)
        {
            query = query.Where(order => order.Status == status);
        }

        var projected = OrderSorting.Map
            .Apply(query, request.Sort)
            .Select(order => new OrderListItemDto
            {
                Number = order.Number,
                Status = order.Status,
                PlacedAt = order.PlacedAt,
                Total = order.Total,
                Currency = order.Currency,
                ItemCount = order.Items.Sum(item => item.Quantity),
            });

        return queryExecutor.ToPagedResultAsync(
            projected,
            request.NormalizedPage,
            request.NormalizedPageSize,
            cancellationToken);
    }
}