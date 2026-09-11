namespace ZeldaArena.Application.Features.Orders.Queries.GetOrderDetails;

/// <summary>
/// Позиция заказа — снапшот названия и цены на момент оформления (docs/SPEC.md §6):
/// заказ не меняется задним числом вслед за каталогом.
/// </summary>
public sealed record OrderLineDto(string ProductName, decimal UnitPrice, int Quantity)
{
    public decimal LineTotal => UnitPrice * Quantity;
}