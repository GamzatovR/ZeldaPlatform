using ZeldaArena.Application.Common.Messaging;
using ZeldaArena.Application.Common.Models;

namespace ZeldaArena.Application.Features.Admin.Shop.Queries.GetProductsForAdmin;

/// <summary>
/// Таблица товаров — <c>/admin/products</c> (docs/SPEC.md §9.4, п. 6). В отличие
/// от каталога показывает и снятые с продажи.
/// </summary>
public sealed record GetProductsForAdminQuery : FilterBase, IQuery<PagedResult<AdminProductRowDto>>
{
    public string? Search { get; init; }

    public Guid? CategoryId { get; init; }

    public bool? IsActive { get; init; }

    /// <summary>Только товары, которых нет на складе, — очередь пополнения.</summary>
    public bool? OutOfStock { get; init; }
}