namespace ZeldaArena.Application.Features.Admin.Orders.Queries.GetOrderForAdmin;

public sealed record AdminOrderItemDto(string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);