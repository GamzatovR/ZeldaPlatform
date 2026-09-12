using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Carts.Commands.ChangeCartItemQuantity;

public sealed record ChangeCartItemQuantityCommand(Guid ProductId, int Quantity) : ICommand<CartSummaryDto>;