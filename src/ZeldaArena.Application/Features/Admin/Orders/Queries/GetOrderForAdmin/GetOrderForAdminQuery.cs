using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Admin.Orders.Queries.GetOrderForAdmin;

/// <summary>Заказ целиком: позиции, адрес, платежи (docs/SPEC.md §9.4, п. 7).</summary>
public sealed record GetOrderForAdminQuery(string Number) : IQuery<AdminOrderDetailsDto?>;