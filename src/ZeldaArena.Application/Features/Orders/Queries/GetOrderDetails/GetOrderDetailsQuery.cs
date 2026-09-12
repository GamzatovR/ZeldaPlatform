using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Orders.Queries.GetOrderDetails;

public sealed record GetOrderDetailsQuery(string Number) : IQuery<OrderDetailsDto?>;