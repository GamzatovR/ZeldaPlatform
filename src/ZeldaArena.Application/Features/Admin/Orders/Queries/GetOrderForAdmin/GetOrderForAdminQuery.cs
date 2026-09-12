using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Admin.Orders.Queries.GetOrderForAdmin;

/// <summary>Заказ целиком: позиции, адрес, платежи.</summary>
public sealed record GetOrderForAdminQuery(string Number) : IQuery<AdminOrderDetailsDto?>;