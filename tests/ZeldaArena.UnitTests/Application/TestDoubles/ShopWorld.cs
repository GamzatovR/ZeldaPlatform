using ZeldaArena.Application.Features.Carts;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Shop;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

/// <summary>
/// Маленький магазин для тестов сценариев: категории и товары, собранные настоящими
/// доменными методами. Репозитории отдают одни и те же объекты, поэтому изменение
/// остатка, сделанное сценарием, видно следующему запросу — как в базе.
///
/// Навигационные свойства здесь пустые — их заполняет EF Core; проекции обязаны
/// это переживать, и тесты это заодно проверяют.
/// </summary>
internal sealed class ShopWorld
{
    public static readonly DateTimeOffset Now = new(2026, 9, 11, 12, 0, 0, TimeSpan.Zero);

    public ShopWorld()
    {
        Keyboards = AddCategory("keyboards", "Клавиатуры");
        Mice = AddCategory("mice", "Мыши");
    }

    public ProductCategory Keyboards { get; }

    public ProductCategory Mice { get; }

    public List<ProductCategory> Categories { get; } = [];

    public InMemoryRepository<Product> Products { get; } = new();

    public InMemoryRepository<Cart> Carts { get; } = new();

    public RecordingUnitOfWork UnitOfWork { get; } = new();

    /// <summary>Кто сейчас работает: вошедший пользователь, гость по куке — или никто.</summary>
    public StubCurrentUserService CurrentUser { get; } = new();

    public StubGuestCartIdentity Guest { get; } = new();

    public IEnumerable<CartItem> CartItems => Carts.Entities.SelectMany(cart => cart.Items);

    public CartLocator Locator() =>
        new(CurrentUser, Guest, Carts, new InMemoryReadRepository<Cart>(Carts.Entities), new InMemoryQueryExecutor());

    public void SignInAs(Guid userId) => CurrentUser.UserId = userId;

    public void BrowseAsGuest(Guid anonymousId)
    {
        CurrentUser.UserId = null;
        Guest.AnonymousId = anonymousId;
    }

    public Cart? CartOf(Guid? userId = null, Guid? anonymousId = null) =>
        Carts.Entities.SingleOrDefault(cart => userId is not null ? cart.UserId == userId : cart.AnonymousId == anonymousId);

    public ProductCategory AddCategory(string slug, string name)
    {
        var category = ProductCategory.Create(Slug.From(slug), name);
        Categories.Add(category);

        return category;
    }

    public Product AddProduct(
        string name,
        decimal price,
        int stock = 10,
        ProductCategory? category = null,
        bool isActive = true)
    {
        var product = Product.Create(
            Slug.From(name),
            $"ZA-{Products.Entities.Count + 1:D3}",
            name,
            (category ?? Keyboards).Id,
            new Money(price, Money.DefaultCurrency),
            stock,
            imagePath: "/img/shop/products-img1.jpg",
            isActive: isActive);

        Products.AddAsync(product).GetAwaiter().GetResult();

        return product;
    }

    public InMemoryReadRepository<T> Read<T>(IEnumerable<T> source)
        where T : BaseEntity =>
        new(source);

    public InMemoryReadRepository<Product> ReadProducts() => new(Products.Entities);

    public InMemoryReadRepository<ProductCategory> ReadCategories() => new(Categories);
}