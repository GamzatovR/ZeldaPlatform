using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Domain.Shop;

/// <summary>Плоский справочник категорий товара: иерархии в проекте нет (docs/SPEC.md §1).</summary>
public class ProductCategory : BaseEntity
{
    private ProductCategory()
    {
    }

    public Slug Slug { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public static ProductCategory Create(Slug slug, string name)
    {
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new ProductCategory { Slug = slug, Name = name.Trim() };
    }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }
}