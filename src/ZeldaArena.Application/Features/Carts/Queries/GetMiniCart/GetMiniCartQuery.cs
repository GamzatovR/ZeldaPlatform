using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Carts.Queries.GetMiniCart;

/// <summary>Счётчик мини-корзины в шапке.</summary>
public sealed record GetMiniCartQuery : IQuery<CartSummaryDto>;