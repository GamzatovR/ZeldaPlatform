using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Carts.Commands.AddCartItem;

public sealed record AddCartItemCommand(Guid ProductId, int Quantity = 1) : ICommand<CartSummaryDto>;