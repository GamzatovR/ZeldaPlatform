using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Orders.Commands.ExpireAbandonedOrders;

/// <summary>
/// Отменяет брошенные заказы: код оплаты истёк больше
/// <see cref="OrderRules.AbandonedAfterCodeExpiry"/> назад, а новый так и не запросили
/// (docs/adr/ADR-0009). Остаток списан при оформлении, и без этой команды такой заказ
/// держал бы товар бессрочно. Вызывается фоновой службой.
///
/// Отдельная команда, а не метод службы, — как <c>ExpireDueSubscriptionsCommand</c>:
/// сценарий проходит тот же конвейер с транзакцией и аудитом.
/// Возвращает число отменённых заказов.
/// </summary>
public sealed record ExpireAbandonedOrdersCommand : ICommand<int>, IAuditableRequest
{
    public string AuditEntityType => ShopAudit.Order;

    /// <summary>Сценарий работает над многими заказами сразу, одной сущности у него нет.</summary>
    public string? AuditEntityId => null;
}