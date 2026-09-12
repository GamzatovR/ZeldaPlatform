namespace ZeldaArena.Application.Features.Admin.Orders.Queries.GetOrderForAdmin;

/// <summary>
/// Позиция заказа. Название и цена — снапшоты на момент покупки (docs/SPEC.md §6):
/// товар мог подорожать или смениться названием, а в заказе он остаётся прежним.
/// </summary>
public sealed record AdminOrderItemDto(string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);