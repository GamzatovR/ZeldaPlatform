using ZeldaArena.Domain.Enums;

namespace ZeldaArena.Web.Extensions;

/// <summary>
/// Вариант бейджа для статуса заказа — одно соответствие на историю и детали заказа,
/// чтобы «оплачен» не оказался зелёным в одном месте и серым в другом.
/// </summary>
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