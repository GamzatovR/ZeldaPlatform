using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Carts.Queries.GetCart;

/// <summary>Корзина текущего владельца — пользователя или гостя.</summary>
public sealed record GetCartQuery : IQuery<CartDto>;