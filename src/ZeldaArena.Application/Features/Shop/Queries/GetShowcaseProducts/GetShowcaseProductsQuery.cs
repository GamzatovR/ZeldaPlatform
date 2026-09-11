using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Shop.Queries.GetProducts;

namespace ZeldaArena.Application.Features.Shop.Queries.GetShowcaseProducts;

/// <summary>
/// Витрина магазина на главной (docs/SPEC.md §9.3, страница 1: «витрина 4–6 товаров»).
/// Только то, что можно купить прямо сейчас: товар «нет в наличии» на витрине —
/// приглашение, которое тут же отказывает.
/// </summary>
/// <param name="Count">Сколько товаров показать. Ограничивается валидатором.</param>
public sealed record GetShowcaseProductsQuery(int Count = 4) : IQuery<IReadOnlyList<ProductListItemDto>>;