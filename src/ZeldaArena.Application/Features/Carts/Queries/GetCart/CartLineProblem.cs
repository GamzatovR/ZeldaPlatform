namespace ZeldaArena.Application.Features.Carts.Queries.GetCart;

public enum CartLineProblem
{
    None = 0,
    Unavailable = 1,
    OutOfStock = 2,
    InsufficientStock = 3,
}