using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Carts.Commands.RemoveCartItem;

/// <summary>Убрать позицию из корзины (docs/SPEC.md §10.1, сценарий 6).</summary>
public sealed record RemoveCartItemCommand(Guid ProductId) : ICommand<CartSummaryDto>;