using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.Shop;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.UnitTests.Domain.Shop;

public class ProductTests
{
    [Fact]
    public void Sku_is_upper_cased_and_product_is_active_by_default()
    {
        var product = CreateProduct(sku: "kb-hyrule-01");

        product.Sku.ShouldBe("KB-HYRULE-01");
        product.IsActive.ShouldBeTrue();
        product.IsInStock.ShouldBeTrue();
    }

    [Fact]
    public void Negative_stock_is_rejected()
    {
        Should.Throw<InvariantViolationException>(() => CreateProduct(stock: -1))
            .Code.ShouldBe("product.negative_stock");
    }

    [Fact]
    public void Category_is_required()
    {
        Should.Throw<InvariantViolationException>(() => Product.Create(
                Slug.From("keyboard"),
                "KB-1",
                "Клавиатура",
                Guid.Empty,
                new Money(1000m, "RUB"),
                5))
            .Code.ShouldBe("product.category_required");
    }

    [Fact]
    public void Stock_is_decreased_on_order_and_returned_on_cancel()
    {
        var product = CreateProduct(stock: 10);

        product.DecreaseStock(4);
        product.StockQuantity.ShouldBe(6);

        product.IncreaseStock(4);
        product.StockQuantity.ShouldBe(10);
    }

    [Fact]
    public void Selling_more_than_available_is_rejected()
    {
        var product = CreateProduct(stock: 3);

        var exception = Should.Throw<InvariantViolationException>(() => product.DecreaseStock(4));

        exception.Code.ShouldBe("product.insufficient_stock");
        exception.Message.ShouldContain("3");
    }

    private static Product CreateProduct(string sku = "KB-1", int stock = 10) =>
        Product.Create(
            Slug.From("keyboard-hyrule-k1"),
            sku,
            "Клавиатура Hyrule K1",
            Guid.CreateVersion7(),
            new Money(4990m, "RUB"),
            stock);
}