using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Web.Extensions;

public static class OrderStatusBadge
{
    public static string CssFor(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "badge-token badge-token--premium",
        OrderStatus.Paid or OrderStatus.Completed => "badge-token badge-token--success",
        OrderStatus.Shipped => "badge-token badge-token--accent",
        _ => "badge-token",
    };
}