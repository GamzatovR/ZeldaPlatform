using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Features.Admin.Orders.Queries.GetOrdersForAdmin;

namespace ZeldaArena.Web.Areas.Admin.Models.Orders;

public sealed class OrderIndexViewModel
{
    public required GetOrdersForAdminQuery Filter { get; init; }

    public required PagedResult<AdminOrderRowDto> Result { get; init; }

    public string Sort => AdminOrderSorting.Map.Resolve(Filter.Sort);
}