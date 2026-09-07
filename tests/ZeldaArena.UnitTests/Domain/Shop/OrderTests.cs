using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Events;
using ZeldaArena.Domain.Shop;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.UnitTests.Domain.Shop;

public class OrderTests
{
    private static readonly DateTimeOffset Now = new(2026, 3, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Totals_are_calculated_from_the_items()
    {
        var order = PlaceOrder(discount: 500m);

        order.Subtotal.ShouldBe(7000m);
        order.DiscountAmount.ShouldBe(500m);
        order.Total.ShouldBe(6500m);
        order.TotalMoney.ShouldBe(new Money(6500m, "RUB"));
        order.Status.ShouldBe(OrderStatus.Pending);
    }

    [Fact]
    public void Placing_an_order_raises_an_event()
    {
        var order = PlaceOrder();

        order.DomainEvents.OfType<OrderPlacedEvent>().ShouldHaveSingleItem()
            .Total.ShouldBe(7000m);
    }

    [Fact]
    public void Item_keeps_name_and_price_snapshots()
    {
        var order = PlaceOrder();

        var item = order.Items.First();
        item.ProductNameSnapshot.ShouldBe("Клавиатура Hyrule K1");
        item.UnitPrice.ShouldBe(2000m);
        item.LineTotal.ShouldBe(4000m);
    }

    [Fact]
    public void Empty_order_is_rejected()
    {
        Should.Throw<InvariantViolationException>(() => Order.Place(
                Guid.CreateVersion7(),
                "ZA-2026-000001",
                CreateAddress(),
                [],
                Now))
            .Code.ShouldBe("order.empty");
    }

    [Fact]
    public void Discount_cannot_exceed_subtotal()
    {
        Should.Throw<InvariantViolationException>(() => PlaceOrder(discount: 7001m))
            .Code.ShouldBe("order.discount_exceeds_subtotal");
    }

    [Fact]
    public void Negative_discount_is_rejected()
    {
        Should.Throw<InvariantViolationException>(() => PlaceOrder(discount: -1m))
            .Code.ShouldBe("order.negative_discount");
    }

    [Fact]
    public void Order_goes_through_the_full_lifecycle()
    {
        var order = PlaceOrder();

        order.MarkPaid(Now.AddMinutes(5));
        order.IsPaid.ShouldBeTrue();

        order.Ship();
        order.Status.ShouldBe(OrderStatus.Shipped);

        order.Complete();
        order.Status.ShouldBe(OrderStatus.Completed);
    }

    [Fact]
    public void Unpaid_order_cannot_be_shipped()
    {
        Should.Throw<InvariantViolationException>(PlaceOrder().Ship)
            .Code.ShouldBe("order.cannot_ship");
    }

    [Fact]
    public void Order_can_be_canceled_before_shipping()
    {
        var order = PlaceOrder();
        order.MarkPaid(Now);

        order.Cancel(Now.AddHours(1));

        order.Status.ShouldBe(OrderStatus.Canceled);
        order.CanceledAt.ShouldBe(Now.AddHours(1));
    }

    [Fact]
    public void Shipped_order_cannot_be_canceled()
    {
        var order = PlaceOrder();
        order.MarkPaid(Now);
        order.Ship();

        Should.Throw<InvariantViolationException>(() => order.Cancel(Now.AddHours(1)))
            .Code.ShouldBe("order.cannot_cancel");
    }

    [Fact]
    public void Paid_order_amount_cannot_be_changed()
    {
        var order = PlaceOrder();
        order.MarkPaid(Now);

        Should.Throw<InvariantViolationException>(() => order.ApplyDiscount(100m))
            .Code.ShouldBe("order.cannot_change");
    }

    [Fact]
    public void Address_requires_every_field()
    {
        Should.Throw<ArgumentException>(() => new ShippingAddress(
            "Линк",
            "+7 900 000-00-00",
            "RU",
            "Москва",
            "  ",
            "101000"));
    }

    private static Order PlaceOrder(decimal discount = 0m) =>
        Order.Place(
            Guid.CreateVersion7(),
            "ZA-2026-000001",
            CreateAddress(),
            [
                new OrderLine(Guid.CreateVersion7(), "Клавиатура Hyrule K1", 2000m, 2),
                new OrderLine(Guid.CreateVersion7(), "Мышь Kakariko M2", 1000m, 3),
            ],
            Now,
            discount);

    private static ShippingAddress CreateAddress() =>
        new("Линк", "+7 900 000-00-00", "RU", "Москва", "Тверская, 1", "101000");
}