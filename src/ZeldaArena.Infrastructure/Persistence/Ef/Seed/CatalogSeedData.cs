using ZeldaArena.Domain.Shop;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Infrastructure.Persistence.Ef.Seed;

/// <summary>
/// Каталог магазина: 3 плоские категории и 24 товара (docs/SPEC.md §6).
/// Объёма хватает, чтобы фильтр по категории и цене и пагинация по 12 были видны.
/// </summary>
public static class CatalogSeedData
{
    private const string Keyboards = "keyboards";
    private const string Mice = "mice";
    private const string Headsets = "headsets";

    public static IReadOnlyList<ProductCategory> Categories() =>
    [
        ProductCategory.Create(Slug.From(Keyboards), "Клавиатуры"),
        ProductCategory.Create(Slug.From(Mice), "Мыши"),
        ProductCategory.Create(Slug.From(Headsets), "Гарнитуры"),
    ];

    public static IReadOnlyList<Product> Products(IReadOnlyDictionary<string, Guid> categoryIdsBySlug)
    {
        ArgumentNullException.ThrowIfNull(categoryIdsBySlug);

        var products = new List<Product>(24);
        var index = 1;

        foreach (var (categorySlug, model, price, stock) in Definitions())
        {
            var sku = $"ZA-{categorySlug[..2].ToUpperInvariant()}-{index:D3}";

            products.Add(Product.Create(
                Slug.From(model),
                sku,
                model,
                categoryIdsBySlug[categorySlug],
                new Money(price, Money.DefaultCurrency),
                stock,
                description: $"{model}: киберспортивная периферия серии ZeldaArena.",
                imagePath: ImageFor(index),
                // Один товар закончился намеренно: пустой остаток нужен, чтобы
                // фильтр «в наличии» и проверка остатка в корзине были видны.
                isActive: true));

            index++;
        }

        return products;
    }

    /// <summary>
    /// В макете четыре фотографии товаров, они и раздаются по кругу. Статический путь
    /// от корня сайта отличается от имени загруженного файла, поэтому витрина понимает
    /// оба вида: товарам из админки (Фаза 9) картинку положит <c>IFileStorage</c>.
    /// </summary>
    private static string ImageFor(int index) => $"/img/shop/products-img{((index - 1) % 4) + 1}.jpg";

    private static IEnumerable<(string Category, string Model, decimal Price, int Stock)> Definitions()
    {
        yield return (Keyboards, "Hyrule K1 Compact", 6990m, 24);
        yield return (Keyboards, "Hyrule K2 TKL", 8490m, 15);
        yield return (Keyboards, "Hyrule K3 Full", 9990m, 9);
        yield return (Keyboards, "Kakariko KB60", 5490m, 31);
        yield return (Keyboards, "Kakariko KB80", 7290m, 12);
        yield return (Keyboards, "Gerudo Sand Edition", 11490m, 4);
        yield return (Keyboards, "Zora Silent Switch", 8990m, 18);
        yield return (Keyboards, "Goron Heavy Frame", 12990m, 0);

        yield return (Mice, "Sheikah S1 Wireless", 5990m, 40);
        yield return (Mice, "Sheikah S2 Pro", 7490m, 22);
        yield return (Mice, "Rito Light 49g", 6790m, 17);
        yield return (Mice, "Rito Light XL", 7190m, 11);
        yield return (Mice, "Deku Grip Ambi", 4290m, 35);
        yield return (Mice, "Lynel Claw", 8990m, 6);
        yield return (Mice, "Korok Mini", 3490m, 48);
        yield return (Mice, "Hylian Precision 8K", 10990m, 3);

        yield return (Headsets, "Ocarina H1", 9490m, 20);
        yield return (Headsets, "Ocarina H2 Wireless", 13990m, 8);
        yield return (Headsets, "Temple Sound Open", 15990m, 5);
        yield return (Headsets, "Temple Sound Closed", 14490m, 7);
        yield return (Headsets, "Faron Studio Mic", 11990m, 13);
        yield return (Headsets, "Eldin Bass Edition", 10490m, 16);
        yield return (Headsets, "Lanayru Esports Pro", 17990m, 2);
        yield return (Headsets, "Skyloft Lite", 6490m, 29);
    }
}