using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Features.Shop.Queries.GetProducts;

namespace ZeldaArena.Application.Features.Shop.Queries.GetShowcaseProducts;

public sealed record GetShowcaseProductsQuery(int Count = 4) : IQuery<IReadOnlyList<ProductListItemDto>>;