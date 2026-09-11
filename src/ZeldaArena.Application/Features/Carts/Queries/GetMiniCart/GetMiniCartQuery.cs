using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Carts.Queries.GetMiniCart;

/// <summary>Счётчик мини-корзины в шапке (docs/SPEC.md §10.2, <c>MiniCart</c>).</summary>
public sealed record GetMiniCartQuery : IQuery<CartSummaryDto>;