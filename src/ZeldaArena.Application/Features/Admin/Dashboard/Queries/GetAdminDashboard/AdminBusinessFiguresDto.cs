namespace ZeldaArena.Application.Features.Admin.Dashboard.Queries.GetAdminDashboard;

/// <summary>
/// Деловые показатели дашборда — только для администратора (docs/SPEC.md §8.1).
///
/// Выручка — сумма подтверждённых платежей: и за подписки, и за заказы. Возвращённый
/// платёж (отмена оплаченного заказа) в неё не входит — деньги ушли обратно покупателю.
/// </summary>
public sealed record AdminBusinessFiguresDto
{
    public int UserCount { get; init; }

    public int ActiveSubscriptions { get; init; }

    public decimal RevenueLast30Days { get; init; }

    public decimal RevenueTotal { get; init; }

    public string Currency { get; init; } = string.Empty;

    /// <summary>Оплаченные, но ещё не отправленные заказы — очередь работы магазина.</summary>
    public int OrdersToShip { get; init; }

    public IReadOnlyList<AdminRecentOrderDto> RecentOrders { get; init; } = [];
}