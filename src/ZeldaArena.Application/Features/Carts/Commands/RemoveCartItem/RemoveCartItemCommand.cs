using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Carts.Commands.RemoveCartItem;

/// <summary>Убрать позицию из корзины.</summary>
public sealed record RemoveCartItemCommand(Guid ProductId) : ICommand<CartSummaryDto>;