using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Orders.Queries.GetOrderDetails;

/// <summary>
/// Детали заказа по номеру (docs/SPEC.md §9.3, п. 15). Чужой и несуществующий заказ —
/// одинаковый <c>null</c> (§15, IDOR).
/// </summary>
public sealed record GetOrderDetailsQuery(string Number) : IQuery<OrderDetailsDto?>;