using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Features.Carts;
using ZeldaArena.Application.Features.Carts.Commands.AddCartItem;
using ZeldaArena.Application.Features.Orders;
using ZeldaArena.Application.Features.Orders.Commands.CancelOrder;
using ZeldaArena.Application.Features.Orders.Commands.ExpireAbandonedOrders;
using ZeldaArena.Application.Features.Orders.Commands.PlaceOrder;
using ZeldaArena.Application.Features.Orders.Queries.GetMyOrders;
using ZeldaArena.Application.Features.Orders.Queries.GetOrderDetails;
using ZeldaArena.Application.Features.Payments;
using ZeldaArena.Application.Features.Payments.Commands.CancelPayment;
using ZeldaArena.Application.Features.Payments.Commands.ConfirmPayment;
using ZeldaArena.Application.Features.Payments.Queries.GetPaymentState;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Shop;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Shop;

/// <summary>
/// Обвязка сценариев заказа: магазин (<see cref="ShopWorld"/>), платежи, подменённые
/// провайдер, коды и почта, часы, которые сдвигаются между шагами. Хендлеры
/// собираются настоящие — те же, что в приложении.
/// </summary>
internal sealed class OrderScenarioFixture
{
    public const string ValidCardNumber = "4242424242424242";

    private DateTimeOffset _now = ShopWorld.Now;

    public OrderScenarioFixture() => World.SignInAs(Buyer);

    public ShopWorld World { get; } = new();

    public Guid Buyer { get; } = Guid.CreateVersion7();

    public InMemoryRepository<Order> Orders { get; } = new();

    public InMemoryRepository<Payment> Payments { get; } = new();

    public StubPaymentGateway Gateway { get; } = new();

    public FixedConfirmationCodeProtector Codes { get; } = new();

    public RecordingBillingEmailSender Email { get; } = new();

    public RecordingSignInService SignIn { get; } = new();

    public Order SingleOrder => Orders.Entities.Single();

    public Payment SinglePayment => Payments.Entities.Single();

    public void Advance(TimeSpan by) => _now += by;

    public Task<Result<CartSummaryDto>> AddToCartAsync(Product product, int quantity = 1) =>
        new AddCartItemCommandHandler(World.Locator(), World.ReadProducts(), World.UnitOfWork)
            .Handle(new AddCartItemCommand(product.Id, quantity), CancellationToken.None);

    public Task<Result<StartPaymentResult>> PlaceAsync(
        string idempotencyKey = "checkout-1",
        string phone = "+7 900 000-00-00") =>
        new PlaceOrderCommandHandler(
                World.CurrentUser,
                World.Locator(),
                World.Products,
                Orders,
                new OrderNumberGenerator(new InMemoryReadRepository<Order>(Orders.Entities), new InMemoryQueryExecutor()),
                Initiator(),
                World.UnitOfWork,
                Clock())
            .Handle(Command(idempotencyKey, phone), CancellationToken.None);

    public Task<Result> ConfirmAsync(string code, Guid? paymentId = null) =>
        new ConfirmPaymentCommandHandler(
                World.CurrentUser,
                Payments,
                new InMemoryRepository<Subscription>(),
                new InMemoryReadRepository<Subscription>([]),
                new InMemoryReadRepository<Plan>([]),
                Orders,
                Cancellation(),
                new InMemoryQueryExecutor(),
                Codes,
                Email,
                SignIn,
                World.UnitOfWork,
                Clock())
            .Handle(new ConfirmPaymentCommand(paymentId ?? SinglePayment.Id, code), CancellationToken.None);

    public Task<Result> CancelPaymentAsync() =>
        new CancelPaymentCommandHandler(
                World.CurrentUser,
                Payments,
                new InMemoryRepository<Subscription>(),
                Orders,
                Cancellation(),
                World.UnitOfWork)
            .Handle(new CancelPaymentCommand(SinglePayment.Id), CancellationToken.None);

    public Task<Result> CancelOrderAsync(string number) =>
        new CancelOrderCommandHandler(
                World.CurrentUser,
                new InMemoryReadRepository<Order>(Orders.Entities),
                Orders,
                Cancellation(),
                new InMemoryQueryExecutor(),
                World.UnitOfWork)
            .Handle(new CancelOrderCommand(number), CancellationToken.None);

    public Task<Result<int>> ExpireAbandonedAsync() =>
        new ExpireAbandonedOrdersCommandHandler(
                new InMemoryReadRepository<Payment>(Payments.Entities),
                Payments,
                Orders,
                Cancellation(),
                new InMemoryQueryExecutor(),
                World.UnitOfWork,
                Clock())
            .Handle(new ExpireAbandonedOrdersCommand(), CancellationToken.None);

    public Task<PagedResult<OrderListItemDto>> MyOrdersAsync(GetMyOrdersQuery? query = null) =>
        new GetMyOrdersQueryHandler(World.CurrentUser, new InMemoryReadRepository<Order>(Orders.Entities), new InMemoryQueryExecutor())
            .Handle(query ?? new GetMyOrdersQuery(), CancellationToken.None);

    public Task<OrderDetailsDto?> DetailsAsync(string number) =>
        new GetOrderDetailsQueryHandler(
                World.CurrentUser,
                new InMemoryReadRepository<Order>(Orders.Entities),
                new InMemoryReadRepository<Payment>(Payments.Entities),
                new InMemoryQueryExecutor())
            .Handle(new GetOrderDetailsQuery(number), CancellationToken.None);

    public Task<PaymentStateDto?> StateAsync() =>
        new GetPaymentStateQueryHandler(
                World.CurrentUser,
                new InMemoryReadRepository<Payment>(Payments.Entities),
                new InMemoryReadRepository<Order>(Orders.Entities),
                new InMemoryQueryExecutor(),
                Clock())
            .Handle(new GetPaymentStateQuery(SinglePayment.Id), CancellationToken.None);

    /// <summary>Корзина → заказ → верный код: исходная точка проверок оплаченного заказа.</summary>
    public async Task<Order> BuyAsync(Product product, int quantity = 1, string idempotencyKey = "checkout-1")
    {
        (await AddToCartAsync(product, quantity)).IsSuccess.ShouldBeTrue();

        var placed = await PlaceAsync(idempotencyKey);
        placed.IsSuccess.ShouldBeTrue();

        (await ConfirmAsync(Codes.Code, placed.Value.PaymentId)).IsSuccess.ShouldBeTrue();

        var orderId = Payments.Entities.Single(payment => payment.Id == placed.Value.PaymentId).OrderId;

        return Orders.Entities.Single(order => order.Id == orderId);
    }

    public static PlaceOrderCommand Command(string idempotencyKey = "checkout-1", string phone = "+7 900 000-00-00") =>
        new(
            "Линк Хайрулов",
            phone,
            "Россия",
            "Москва",
            "ул. Хайрульская, 1",
            "101000",
            ValidCardNumber,
            12,
            2030,
            "123",
            "link@zeldaarena.test",
            idempotencyKey);

    private PaymentInitiator Initiator() =>
        new(
            Payments,
            new InMemoryReadRepository<Payment>(Payments.Entities),
            new InMemoryQueryExecutor(),
            Gateway,
            Codes,
            Email,
            Clock());

    private OrderCancellation Cancellation() =>
        new(
            World.Products,
            Payments,
            new InMemoryReadRepository<Payment>(Payments.Entities),
            World.Locator(),
            new InMemoryQueryExecutor(),
            Clock());

    private FixedDateTimeProvider Clock() => new(_now);
}