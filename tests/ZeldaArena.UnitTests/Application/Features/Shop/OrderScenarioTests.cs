using System.Text.RegularExpressions;

using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Common.Models.Shop;
using ZeldaArena.Application.Features.Orders;
using ZeldaArena.Application.Features.Orders.Commands.PlaceOrder;
using ZeldaArena.Application.Features.Orders.Queries.GetMyOrders;
using ZeldaArena.Application.Features.Payments;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Shop;
using ZeldaArena.Domain.ValueObjects;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Shop;

public class OrderScenarioTests
{
    private readonly OrderScenarioFixture _shop = new();
    private readonly Product _keyboard;
    private readonly Product _mouse;

    public OrderScenarioTests()
    {
        _keyboard = _shop.World.AddProduct("Hyrule K1", 6990m, stock: 5);
        _mouse = _shop.World.AddProduct("Sheikah S1", 5990m, stock: 40, category: _shop.World.Mice);
    }

    [Fact]
    public async Task Placing_an_order_reserves_stock_empties_the_cart_and_sends_a_code()
    {
        await _shop.AddToCartAsync(_keyboard, 2);
        await _shop.AddToCartAsync(_mouse);

        var result = await _shop.PlaceAsync();

        result.IsSuccess.ShouldBeTrue();

        var order = _shop.SingleOrder;
        order.Status.ShouldBe(OrderStatus.Pending);
        order.UserId.ShouldBe(_shop.Buyer);
        order.Total.ShouldBe((2 * 6990m) + 5990m);
        order.Address.City.ShouldBe("Москва");

        _keyboard.StockQuantity.ShouldBe(3);
        _mouse.StockQuantity.ShouldBe(39);
        _shop.World.CartOf(userId: _shop.Buyer)!.IsEmpty.ShouldBeTrue();

        var payment = _shop.SinglePayment;
        payment.Purpose.ShouldBe(PaymentPurpose.Order);
        payment.OrderId.ShouldBe(order.Id);
        payment.Amount.Amount.ShouldBe(order.Total);
        payment.CardLast4.ShouldBe("4242");

        _shop.Email.Single(RecordingBillingEmailSender.LetterKind.PaymentCode).To.ShouldBe("link@zeldaarena.test");
        result.Value.MaskedEmail.ShouldNotContain("link@");
    }

    /// <summary>: в позиции заказа — снапшоты названия и цены, а не ссылки на изменчивый каталог.</summary>
    [Fact]
    public async Task Order_lines_are_snapshots_of_the_current_server_price()
    {
        await _shop.AddToCartAsync(_keyboard);
        _keyboard.ChangePrice(new Money(7490m, Money.DefaultCurrency));

        await _shop.PlaceAsync();

        var line = _shop.SingleOrder.Items.ShouldHaveSingleItem();
        line.UnitPrice.ShouldBe(7490m);
        line.ProductNameSnapshot.ShouldBe("Hyrule K1");
        _shop.Gateway.LastRequest!.Amount.Amount.ShouldBe(7490m);

        _keyboard.ChangePrice(new Money(1m, Money.DefaultCurrency));
        line.UnitPrice.ShouldBe(7490m);
    }

    [Fact]
    public async Task Order_number_is_the_year_and_six_digits()
    {
        await _shop.AddToCartAsync(_keyboard);
        await _shop.PlaceAsync();

        Regex.IsMatch(_shop.SingleOrder.Number, $"^{OrderNumberGenerator.Prefix}-2026-[0-9]{{6}}$").ShouldBeTrue(_shop.SingleOrder.Number);
    }

    /// <summary>Повторная отправка формы не заводит второй заказ и второй платёж.</summary>
    [Fact]
    public async Task Resubmitting_the_same_form_returns_the_same_payment()
    {
        await _shop.AddToCartAsync(_keyboard);

        var first = await _shop.PlaceAsync("same-key");
        var second = await _shop.PlaceAsync("same-key");

        second.Value.PaymentId.ShouldBe(first.Value.PaymentId);
        _shop.Orders.Entities.ShouldHaveSingleItem();
        _shop.Email.Count(RecordingBillingEmailSender.LetterKind.PaymentCode).ShouldBe(1);
    }

    [Fact]
    public async Task An_empty_cart_cannot_be_ordered() =>
        (await _shop.PlaceAsync()).Error.ShouldBe(ShopErrors.CartEmpty);

    /// <summary>Остаток изменился, пока товар лежал в корзине, — ничего не списано и не заведено.</summary>
    [Fact]
    public async Task A_cart_line_above_the_current_stock_blocks_the_order()
    {
        await _shop.AddToCartAsync(_keyboard, 4);
        _keyboard.SetStock(2);

        (await _shop.PlaceAsync()).Error.ShouldBe(ShopErrors.CartHasProblems);

        _shop.Orders.Entities.ShouldBeEmpty();
        _shop.Payments.Entities.ShouldBeEmpty();
        _keyboard.StockQuantity.ShouldBe(2);
    }

    /// <summary>Карта проверяется до любых изменений: отклонённая карта не оставляет заказа.</summary>
    [Fact]
    public async Task A_declined_card_leaves_no_order_and_keeps_the_cart()
    {
        await _shop.AddToCartAsync(_keyboard, 2);
        _shop.Gateway.Declines = true;

        (await _shop.PlaceAsync()).Error.ShouldBe(BillingErrors.CardDeclined);

        _shop.Orders.Entities.ShouldBeEmpty();
        _keyboard.StockQuantity.ShouldBe(5);
        _shop.World.CartOf(userId: _shop.Buyer)!.TotalQuantity.ShouldBe(2);
    }

    /// <summary>Двое купили последнюю единицу: второй получает понятный отказ, а не ошибку сервера.</summary>
    [Fact]
    public async Task A_concurrent_stock_change_is_answered_politely()
    {
        await _shop.AddToCartAsync(_keyboard);
        _shop.World.UnitOfWork.ConflictOnNextSave = true;

        (await _shop.PlaceAsync()).Error.ShouldBe(ShopErrors.StockChanged);
        _shop.Email.Count(RecordingBillingEmailSender.LetterKind.PaymentCode).ShouldBe(0);
    }

    [Fact]
    public async Task The_right_code_marks_the_order_paid_and_sends_a_receipt()
    {
        var order = await _shop.BuyAsync(_keyboard, 2);

        order.Status.ShouldBe(OrderStatus.Paid);
        order.PaidAt.ShouldNotBeNull();

        var receipt = _shop.Email.Single(RecordingBillingEmailSender.LetterKind.OrderReceipt);
        receipt.PlanName.ShouldBe(order.Number);
        receipt.Amount.ShouldBe(order.Total);

        // Оплаченный заказ стоит на своём: остаток остаётся списанным.
        _keyboard.StockQuantity.ShouldBe(3);
    }

    [Fact]
    public async Task A_wrong_code_spends_an_attempt_and_keeps_the_order()
    {
        await _shop.AddToCartAsync(_keyboard);
        await _shop.PlaceAsync();

        (await _shop.ConfirmAsync("000000")).Error.ShouldBe(BillingErrors.WrongCode);

        _shop.SinglePayment.ConfirmationAttemptsLeft.ShouldBe(Payment.MaxAttempts - 1);
        _shop.SingleOrder.Status.ShouldBe(OrderStatus.Pending);
    }

    /// <summary>ADR-0009: исчерпанные попытки сразу отменяют заказ, остаток и товары возвращаются.</summary>
    [Fact]
    public async Task Running_out_of_attempts_cancels_the_order_and_returns_everything()
    {
        await _shop.AddToCartAsync(_keyboard, 2);
        await _shop.PlaceAsync();

        for (var attempt = 1; attempt < Payment.MaxAttempts; attempt++)
        {
            await _shop.ConfirmAsync("000000");
        }

        (await _shop.ConfirmAsync("000000")).Error.ShouldBe(BillingErrors.NoAttemptsLeft);

        _shop.SingleOrder.Status.ShouldBe(OrderStatus.Canceled);
        _keyboard.StockQuantity.ShouldBe(5);
        _shop.World.CartOf(userId: _shop.Buyer)!.Items.ShouldHaveSingleItem().Quantity.ShouldBe(2);
        (await _shop.StateAsync())!.Status.ShouldBe(PaymentStatus.Failed);
    }

    [Fact]
    public async Task An_expired_code_cancels_the_order()
    {
        await _shop.AddToCartAsync(_keyboard);
        await _shop.PlaceAsync();

        _shop.Advance(PaymentPolicy.CodeLifetime + TimeSpan.FromSeconds(1));

        (await _shop.ConfirmAsync(_shop.Codes.Code)).Error.ShouldBe(BillingErrors.CodeExpired);
        _shop.SingleOrder.Status.ShouldBe(OrderStatus.Canceled);
        _keyboard.StockQuantity.ShouldBe(5);
    }

    [Fact]
    public async Task Canceling_the_payment_cancels_the_order_and_refills_the_cart()
    {
        await _shop.AddToCartAsync(_keyboard, 3);
        await _shop.PlaceAsync();

        (await _shop.CancelPaymentAsync()).IsSuccess.ShouldBeTrue();

        _shop.SinglePayment.Status.ShouldBe(PaymentStatus.Canceled);
        _shop.SingleOrder.Status.ShouldBe(OrderStatus.Canceled);
        _keyboard.StockQuantity.ShouldBe(5);
        _shop.World.CartOf(userId: _shop.Buyer)!.TotalQuantity.ShouldBe(3);
    }

    /// <summary>Товары возвращаются в корзину не больше остатка — как при слиянии корзин.</summary>
    [Fact]
    public async Task Returned_items_are_capped_by_the_stock_that_is_left()
    {
        await _shop.AddToCartAsync(_keyboard, 3);
        await _shop.PlaceAsync();

        // Пока заказ ждал оплаты, покупатель положил в корзину ещё две штуки,
        // а остаток склада (две штуки) раскупили другие.
        await _shop.AddToCartAsync(_keyboard, 2);
        _keyboard.SetStock(0);

        await _shop.CancelPaymentAsync();

        // На склад вернулись три штуки заказа; в корзине 2 + 3 = 5 не помещается — остаётся 3.
        _keyboard.StockQuantity.ShouldBe(3);
        _shop.World.CartOf(userId: _shop.Buyer)!.TotalQuantity.ShouldBe(3);
    }

    [Fact]
    public async Task The_buyer_cancels_an_unpaid_order_without_refilling_the_cart()
    {
        await _shop.AddToCartAsync(_keyboard, 2);
        await _shop.PlaceAsync();

        (await _shop.CancelOrderAsync(_shop.SingleOrder.Number.ToLowerInvariant())).IsSuccess.ShouldBeTrue();

        _shop.SingleOrder.Status.ShouldBe(OrderStatus.Canceled);
        _keyboard.StockQuantity.ShouldBe(5);
        _shop.SinglePayment.Status.ShouldBe(PaymentStatus.Failed);
        _shop.SinglePayment.FailureReason.ShouldBe(OrderCancellation.PaymentFailureReason);
        _shop.World.CartOf(userId: _shop.Buyer)!.IsEmpty.ShouldBeTrue();
    }

    /// <summary>ADR-0009: оплаченный заказ покупатель не отменяет — это решение магазина (Фаза 9).</summary>
    [Fact]
    public async Task A_paid_order_cannot_be_canceled_by_the_buyer()
    {
        var order = await _shop.BuyAsync(_keyboard);

        (await _shop.CancelOrderAsync(order.Number)).Error.ShouldBe(ShopErrors.OrderNotCancelable);
        order.Status.ShouldBe(OrderStatus.Paid);
    }

    [Fact]
    public async Task A_canceled_order_cannot_be_paid()
    {
        await _shop.AddToCartAsync(_keyboard);
        await _shop.PlaceAsync();
        var number = _shop.SingleOrder.Number;

        _shop.SingleOrder.Cancel(ShopWorld.Now);

        (await _shop.ConfirmAsync(_shop.Codes.Code)).Error.ShouldBe(ShopErrors.OrderNotPayable);
        _shop.SinglePayment.Status.ShouldBe(PaymentStatus.Failed);
        (await _shop.DetailsAsync(number))!.CanContinuePayment.ShouldBeFalse();
    }

    /// <summary>IDOR: чужой заказ неотличим от несуществующего.</summary>
    [Fact]
    public async Task Another_buyer_can_neither_see_nor_cancel_the_order()
    {
        await _shop.AddToCartAsync(_keyboard);
        await _shop.PlaceAsync();
        var number = _shop.SingleOrder.Number;

        _shop.World.SignInAs(Guid.CreateVersion7());

        (await _shop.DetailsAsync(number)).ShouldBeNull();
        (await _shop.CancelOrderAsync(number)).Error.ShouldBe(ShopErrors.OrderNotFound);
        (await _shop.MyOrdersAsync()).TotalCount.ShouldBe(0);
        _shop.SingleOrder.Status.ShouldBe(OrderStatus.Pending);
    }

    [Fact]
    public async Task Details_show_snapshots_and_the_payment_to_continue()
    {
        await _shop.AddToCartAsync(_keyboard, 2);
        await _shop.PlaceAsync();

        var details = (await _shop.DetailsAsync(_shop.SingleOrder.Number)).ShouldNotBeNull();

        details.Lines.ShouldHaveSingleItem().LineTotal.ShouldBe(2 * 6990m);
        details.Recipient.ShouldBe("Линк Хайрулов");
        details.PendingPaymentId.ShouldBe(_shop.SinglePayment.Id);
        details.CanCancel.ShouldBeTrue();
        details.CanContinuePayment.ShouldBeTrue();
    }

    [Fact]
    public async Task Order_history_filters_by_status_and_sorts_by_total()
    {
        var paid = await _shop.BuyAsync(_keyboard, 2, "first");
        await _shop.AddToCartAsync(_mouse);
        await _shop.PlaceAsync("second");

        var all = await _shop.MyOrdersAsync(new GetMyOrdersQuery { Sort = OrderSorting.TotalAscending });
        all.Items.Select(order => order.Total).ShouldBe([5990m, 2 * 6990m]);
        all.Items.Last().ItemCount.ShouldBe(2);

        var onlyPaid = await _shop.MyOrdersAsync(new GetMyOrdersQuery { Status = OrderStatus.Paid });
        onlyPaid.Items.ShouldHaveSingleItem().Number.ShouldBe(paid.Number);
    }

    [Fact]
    public async Task Abandoned_orders_are_canceled_by_the_background_sweep()
    {
        await _shop.AddToCartAsync(_keyboard, 2);
        await _shop.PlaceAsync("abandoned");
        var abandoned = _shop.SingleOrder;

        _shop.Advance(PaymentPolicy.CodeLifetime + OrderRules.AbandonedAfterCodeExpiry + TimeSpan.FromMinutes(1));

        await _shop.AddToCartAsync(_mouse);
        await _shop.PlaceAsync("fresh");
        var fresh = _shop.Orders.Entities.Single(order => order.Id != abandoned.Id);

        (await _shop.ExpireAbandonedAsync()).Value.ShouldBe(1);

        abandoned.Status.ShouldBe(OrderStatus.Canceled);
        fresh.Status.ShouldBe(OrderStatus.Pending);
        _keyboard.StockQuantity.ShouldBe(5);
        _shop.World.CartOf(userId: _shop.Buyer)!.Items.ShouldHaveSingleItem().ProductId.ShouldBe(_keyboard.Id);
    }

    [Fact]
    public async Task A_code_expired_within_the_grace_period_is_left_alone()
    {
        await _shop.AddToCartAsync(_keyboard);
        await _shop.PlaceAsync();

        _shop.Advance(PaymentPolicy.CodeLifetime + TimeSpan.FromMinutes(5));

        (await _shop.ExpireAbandonedAsync()).Value.ShouldBe(0);
        _shop.SingleOrder.Status.ShouldBe(OrderStatus.Pending);
    }

    [Fact]
    public async Task Resubmitting_the_code_after_payment_changes_nothing()
    {
        var order = await _shop.BuyAsync(_keyboard);

        (await _shop.ConfirmAsync(_shop.Codes.Code)).Error.ShouldBe(BillingErrors.AlreadyProcessed);

        order.Status.ShouldBe(OrderStatus.Paid);
        _keyboard.StockQuantity.ShouldBe(4);
        _shop.Email.Count(RecordingBillingEmailSender.LetterKind.OrderReceipt).ShouldBe(1);
    }

    /// <summary>Параллельное изменение при подтверждении — понятный отказ, чек не уходит.</summary>
    [Fact]
    public async Task A_concurrent_change_while_confirming_is_answered_politely()
    {
        await _shop.AddToCartAsync(_keyboard);
        await _shop.PlaceAsync();
        _shop.World.UnitOfWork.ConflictOnNextSave = true;

        (await _shop.ConfirmAsync(_shop.Codes.Code)).Error.ShouldBe(BillingErrors.ConcurrentChange);
        _shop.Email.Count(RecordingBillingEmailSender.LetterKind.OrderReceipt).ShouldBe(0);
    }

    [Fact]
    public async Task A_concurrent_change_while_canceling_the_order_is_answered_politely()
    {
        await _shop.AddToCartAsync(_keyboard);
        await _shop.PlaceAsync();
        _shop.World.UnitOfWork.ConflictOnNextSave = true;

        (await _shop.CancelOrderAsync(_shop.SingleOrder.Number)).Error.ShouldBe(ShopErrors.OrderChangedConcurrently);
    }

    [Fact]
    public async Task A_concurrent_change_while_canceling_the_payment_is_answered_politely()
    {
        await _shop.AddToCartAsync(_keyboard);
        await _shop.PlaceAsync();
        _shop.World.UnitOfWork.ConflictOnNextSave = true;

        (await _shop.CancelPaymentAsync()).Error.ShouldBe(BillingErrors.ConcurrentChange);
    }

    /// <summary>Одно правило срока на форму и серверный валидатор (форма больше не пропускает истёкшую карту в 500).</summary>
    [Fact]
    public void Current_month_card_is_valid_and_last_year_is_expired()
    {
        var today = DateTime.UtcNow;

        CardPaymentRules.IsExpired(today.Month, today.Year).ShouldBeFalse();
        CardPaymentRules.IsExpired(12, today.Year - 1).ShouldBeTrue();
        CardPaymentRules.IsExpired(13, today.Year).ShouldBeTrue();
        CardPaymentRules.IsExpired(1, 0).ShouldBeTrue();
    }

    [Theory]
    [InlineData("1234")]
    [InlineData("   ")]
    [InlineData("   12")]
    public void A_too_short_phone_is_rejected_before_the_domain(string phone) =>
        new PlaceOrderCommandValidator().Validate(OrderScenarioFixture.Command(phone: phone))
            .Errors.ShouldContain(failure => failure.PropertyName == nameof(PlaceOrderCommand.Phone));

    [Fact]
    public void Checkout_shares_the_card_rules_with_subscriptions()
    {
        var validator = new PlaceOrderCommandValidator();

        validator.Validate(OrderScenarioFixture.Command()).IsValid.ShouldBeTrue();
        validator.Validate(OrderScenarioFixture.Command() with { CardNumber = "4242424242424243" })
            .Errors.ShouldContain(failure => failure.PropertyName == nameof(PlaceOrderCommand.CardNumber));
        validator.Validate(OrderScenarioFixture.Command() with { Street = new string('a', PlaceOrderCommandValidator.MaxStreetLength + 1) })
            .IsValid.ShouldBeFalse();
    }
}