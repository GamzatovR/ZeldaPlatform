using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Carts.Commands.AddCartItem;

/// <summary>
/// Положить товар в корзину (docs/SPEC.md §9.3, п. 12; §10.1, сценарий 5).
///
/// Цены в команде нет и быть не может: она берётся у товара на сервере (§15, §20 п. 7).
/// Корзины и владельца тоже нет — их определяет <see cref="CartLocator"/>.
/// </summary>
public sealed record AddCartItemCommand(Guid ProductId, int Quantity = 1) : ICommand<CartSummaryDto>;