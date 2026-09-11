using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Carts.Commands.MergeGuestCart;

/// <summary>
/// Влить гостевую корзину в корзину вошедшего пользователя (docs/SPEC.md §14.1, §19:
/// «корзина гостя сливается с пользовательской при входе»). Отправляет её
/// <c>CartCookieMiddleware</c> на первом запросе после входа — одним местом для всех
/// путей входа: по паролю, вторым фактором и кодом восстановления.
///
/// Параметров нет: и пользователь, и гость берутся из проверенного контекста запроса.
/// </summary>
public sealed record MergeGuestCartCommand : ICommand<CartSummaryDto>;