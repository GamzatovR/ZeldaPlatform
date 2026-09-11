using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Carts.Queries.GetCart;

/// <summary>Корзина текущего владельца — пользователя или гостя (docs/SPEC.md §9.3, п. 13).</summary>
public sealed record GetCartQuery : IQuery<CartDto>;