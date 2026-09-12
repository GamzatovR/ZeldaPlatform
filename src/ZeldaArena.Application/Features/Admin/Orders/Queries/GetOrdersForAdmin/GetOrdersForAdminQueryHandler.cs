using MediatR;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Admin.Orders.Queries.GetOrdersForAdmin;

public sealed class GetOrdersForAdminQueryHandler(
    IReadRepository<Order> orders,
    IQueryExecutor queryExecutor)
    : IRequestHandler<GetOrdersForAdminQuery, PagedResult<AdminOrderRowDto>>
{
    public Task<PagedResult<AdminOrderRowDto>> Handle(
        GetOrdersForAdminQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = orders.Query();

        if (request.Status is { } status)
        {
            query = query.Where(order => order.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = request.Search.Trim().ToLowerInvariant();

            query = query.Where(order => order.Number.ToLower().Contains(pattern)
                || order.Address.Recipient.ToLower().Contains(pattern));
        }

        var rows = AdminOrderSorting.Map
            .Apply(query, request.Sort)
            .Select(order => new AdminOrderRowDto
            {
                Number = order.Number,
                Status = order.Status,
                PlacedAt = order.PlacedAt,
                Total = order.Total,
                Currency = order.Currency,
                ItemCount = order.Items.Count(),
                Recipient = order.Address.Recipient,
                City = order.Address.City,
            });

        return queryExecutor.ToPagedResultAsync(
            rows,
            request.NormalizedPage,
            request.NormalizedPageSize,
            cancellationToken);
    }
}