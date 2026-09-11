using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Carts.Commands.ChangeCartItemQuantity;

/// <summary>
/// Новое количество позиции — абсолютное, а не приращение (docs/SPEC.md §9.3, п. 13;
/// §10.1, сценарий 6). Ноль не означает удаление: для этого есть отдельная команда,
/// и опечатка в поле не должна молча выбрасывать товар из корзины.
/// </summary>
public sealed record ChangeCartItemQuantityCommand(Guid ProductId, int Quantity) : ICommand<CartSummaryDto>;