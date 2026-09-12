namespace ZeldaArena.Application.Features.Orders.Queries.GetOrderDetails;

public sealed record OrderLineDto(string ProductName, decimal UnitPrice, int Quantity)
{
    public decimal LineTotal => UnitPrice * Quantity;
}