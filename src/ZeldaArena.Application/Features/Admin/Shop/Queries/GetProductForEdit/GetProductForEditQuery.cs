using ZeldaArena.Application.Common.Messaging;

namespace ZeldaArena.Application.Features.Admin.Shop.Queries.GetProductForEdit;

/// <summary>Карточка товара и список категорий для формы.</summary>
public sealed record GetProductForEditQuery(Guid Id) : IQuery<ProductEditDto?>;