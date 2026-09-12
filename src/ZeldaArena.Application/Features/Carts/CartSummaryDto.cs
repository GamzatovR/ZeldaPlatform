namespace ZeldaArena.Application.Features.Carts;

public sealed record CartSummaryDto(int ItemCount)
{
    public static CartSummaryDto Empty { get; } = new(0);
}