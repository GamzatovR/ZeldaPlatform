using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Carts.Commands.MergeGuestCart;

public sealed record MergeGuestCartCommand : ICommand<CartSummaryDto>;