using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Shop.Queries.GetShopFilterOptions;

/// <summary>Категории для выпадающего списка фильтра магазина.</summary>
public sealed record GetShopFilterOptionsQuery : IQuery<IReadOnlyList<CategoryOptionDto>>;