using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.Shop;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.UnitTests.Domain.Shop;

public class CartTests
{
    private static readonly Guid CategoryId = Guid.CreateVersion7();

    [Fact]
    public void New_cart_is_empty()
    {
        var cart = Cart.ForUser(Guid.CreateVersion7());

        cart.IsEmpty.ShouldBeTrue();
        cart.Subtotal.ShouldBe(Money.Zero());
    }

    [Fact]
    public void Guest_cart_requires_anonymous_id()
    {
        Should.Throw<InvariantViolationException>(() => Cart.ForGuest(Guid.Empty))
            .Code.ShouldBe("cart.owner_required");
    }

    [Fact]
    public void Price_is_taken_from_the_product_not_from_the_request()
    {
        var cart = Cart.ForUser(Guid.CreateVersion7());
        var product = CreateProduct(price: 4990m, stock: 10);

        cart.AddItem(product, 2);

        cart.Items.ShouldHaveSingleItem().PriceSnapshot.ShouldBe(4990m);
        cart.Subtotal.ShouldBe(new Money(9980m, "RUB"));
        cart.TotalQuantity.ShouldBe(2);
    }

    [Fact]
    public void Adding_the_same_product_increases_quantity()
    {
        var cart = Cart.ForUser(Guid.CreateVersion7());
        var product = CreateProduct(stock: 10);

        cart.AddItem(product, 2);
        cart.AddItem(product, 3);

        cart.Items.ShouldHaveSingleItem().Quantity.ShouldBe(5);
    }

    [Fact]
    public void Quantity_cannot_exceed_stock()
    {
        var cart = Cart.ForUser(Guid.CreateVersion7());
        var product = CreateProduct(stock: 3);

        Should.Throw<InvariantViolationException>(() => cart.AddItem(product, 4))
            .Code.ShouldBe("cart.insufficient_stock");
    }

    [Fact]
    public void Accumulated_quantity_is_also_checked_against_stock()
    {
        var cart = Cart.ForUser(Guid.CreateVersion7());
        var product = CreateProduct(stock: 3);
        cart.AddItem(product, 2);

        Should.Throw<InvariantViolationException>(() => cart.AddItem(product, 2))
            .Code.ShouldBe("cart.insufficient_stock");
    }

    [Fact]
    public void Inactive_product_cannot_be_added()
    {
        var cart = Cart.ForUser(Guid.CreateVersion7());
        var product = CreateProduct(stock: 5);
        product.Deactivate();

        Should.Throw<InvariantViolationException>(() => cart.AddItem(product, 1))
            .Code.ShouldBe("cart.product_unavailable");
    }

    [Fact]
    public void Quantity_below_one_is_rejected()
    {
        var cart = Cart.ForUser(Guid.CreateVersion7());
        var product = CreateProduct(stock: 5);

        Should.Throw<InvariantViolationException>(() => cart.AddItem(product, 0))
            .Code.ShouldBe("cart.invalid_quantity");
    }

    [Fact]
    public void Quantity_can_be_changed_and_item_removed()
    {
        var cart = Cart.ForUser(Guid.CreateVersion7());
        var product = CreateProduct(price: 1000m, stock: 10);
        cart.AddItem(product, 5);

        cart.ChangeQuantity(product, 2);

        cart.Subtotal.ShouldBe(new Money(2000m, "RUB"));

        cart.RemoveItem(product.Id);

        cart.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void Removing_a_missing_item_is_rejected()
    {
        var cart = Cart.ForUser(Guid.CreateVersion7());

        Should.Throw<InvariantViolationException>(() => cart.RemoveItem(Guid.CreateVersion7()))
            .Code.ShouldBe("cart.item_not_found");
    }

    [Fact]
    public void Guest_cart_merges_into_user_cart_within_stock_limits()
    {
        var product = CreateProduct(price: 500m, stock: 4);
        var userCart = Cart.ForUser(Guid.CreateVersion7());
        userCart.AddItem(product, 3);

        var guestCart = Cart.ForGuest(Guid.CreateVersion7());
        guestCart.AddItem(product, 3);

        userCart.MergeFrom(guestCart, new Dictionary<Guid, Product> { [product.Id] = product });

        // 3 + 3 = 6, но на складе только 4 — берётся остаток, а не сумма.
        userCart.Items.ShouldHaveSingleItem().Quantity.ShouldBe(4);
    }

    [Fact]
    public void Merge_skips_products_that_are_gone()
    {
        var product = CreateProduct(stock: 5);
        var guestCart = Cart.ForGuest(Guid.CreateVersion7());
        guestCart.AddItem(product, 1);

        var userCart = Cart.ForUser(Guid.CreateVersion7());
        userCart.MergeFrom(guestCart, new Dictionary<Guid, Product>());

        userCart.IsEmpty.ShouldBeTrue();
    }

    private static Product CreateProduct(decimal price = 2500m, int stock = 10) =>
        Product.Create(
            Slug.From($"product-{Guid.CreateVersion7()}"),
            $"SKU-{Guid.CreateVersion7():N}",
            "Клавиатура Hyrule K1",
            CategoryId,
            new Money(price, "RUB"),
            stock);
}