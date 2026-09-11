namespace ZeldaArena.Application.Features.Orders;

/// <summary>
/// Типы сущностей для записей аудита по сценариям магазина (docs/SPEC.md §13).
/// Оформление и отмена заказа — в списке обязательных к записи событий §8.2.
/// </summary>
public static class ShopAudit
{
    public const string Order = nameof(Domain.Shop.Order);
}