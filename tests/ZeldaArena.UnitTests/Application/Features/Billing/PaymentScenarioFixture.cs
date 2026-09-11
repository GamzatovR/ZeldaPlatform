using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Features.Carts;
using ZeldaArena.Application.Features.Orders;
using ZeldaArena.Application.Features.Payments;
using ZeldaArena.Application.Features.Payments.Commands.CancelPayment;
using ZeldaArena.Application.Features.Payments.Commands.ConfirmPayment;
using ZeldaArena.Application.Features.Payments.Commands.ResendPaymentCode;
using ZeldaArena.Application.Features.Payments.Commands.StartSubscriptionPayment;
using ZeldaArena.Application.Features.Payments.Queries.GetPaymentState;
using ZeldaArena.Domain.Billing;
using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Constants;
using ZeldaArena.Domain.Shop;
using ZeldaArena.Domain.ValueObjects;
using ZeldaArena.UnitTests.Application.TestDoubles;

namespace ZeldaArena.UnitTests.Application.Features.Billing;

/// <summary>
/// Общая обвязка сценариев оплаты: тарифы, репозитории и подменённые порты.
///
/// Порты подменены реализациями, а не моками, там где проверяется поведение
/// (как в Фазах 2 и 3): <see cref="InMemoryRepository{TEntity}"/> действительно
/// хранит и удаляет, поэтому «заявка на подписку убрана» проверяется по составу
/// хранилища, а не по факту вызова метода.
///
/// Часы сдвигаются между шагами — на них держатся истечение кода и пауза
/// между отправками.
/// </summary>
internal sealed class PaymentScenarioFixture
{
    public const string ValidCardNumber = "4242424242424242";

    public static readonly DateTimeOffset Start = new(2026, 9, 9, 12, 0, 0, TimeSpan.Zero);

    private DateTimeOffset _now = Start;

    public PaymentScenarioFixture()
    {
        Monthly = Plan.Create(PlanCodes.ProMonth, "Pro на месяц", Price, durationDays: 30);
        Yearly = Plan.Create(PlanCodes.ProYear, "Pro на год", new Money(2490m, Money.DefaultCurrency), 365);
        Free = Plan.Create(PlanCodes.Free, "Бесплатный", Money.Zero(), durationDays: 0);

        Plans = new InMemoryReadRepository<Plan>([Monthly, Yearly, Free]);
    }

    public static Money Price { get; } = new(299m, Money.DefaultCurrency);

    public Guid UserId { get; } = Guid.CreateVersion7();

    public Plan Monthly { get; }

    public Plan Yearly { get; }

    public Plan Free { get; }

    public InMemoryReadRepository<Plan> Plans { get; }

    public InMemoryRepository<Payment> Payments { get; } = new();

    public InMemoryRepository<Subscription> Subscriptions { get; } = new();

    /// <summary>
    /// Заказов в сценариях подписки нет, но хендлеры оплаты общие для обоих назначений
    /// (§7.6); сценарии заказа проверяются отдельно, на <c>ShopWorld</c>.
    /// </summary>
    public InMemoryRepository<Order> Orders { get; } = new();

    public InMemoryRepository<Product> Products { get; } = new();

    public InMemoryRepository<Cart> Carts { get; } = new();

    public StubPaymentGateway Gateway { get; } = new();

    public FixedConfirmationCodeProtector Codes { get; } = new();

    public RecordingBillingEmailSender Email { get; } = new();

    public RecordingUnitOfWork UnitOfWork { get; } = new();

    /// <summary>Перевыпуск cookie после оплаты: без него бейдж Premium отстал бы на пять минут.</summary>
    public RecordingSignInService SignIn { get; } = new();

    /// <summary>Кто платит. Аноним подставляется отдельным тестом.</summary>
    public Guid? SignedInUserId { get; set; }

    public string SentCode => Email.Single(RecordingBillingEmailSender.LetterKind.PaymentCode).Code!;

    public Payment SinglePayment => Payments.Entities.Single();

    public void Advance(TimeSpan by) => _now += by;

    public Task<Result<StartPaymentResult>> StartAsync(
        Guid? planId = null,
        string? cardNumber = null,
        string idempotencyKey = "form-1",
        int expiryYear = 2030) =>
        new StartSubscriptionPaymentCommandHandler(
            CurrentUser(),
            Plans,
            Subscriptions,
            Initiator(),
            UnitOfWork,
            Clock())
        .Handle(
            new StartSubscriptionPaymentCommand(
                planId ?? Monthly.Id,
                cardNumber ?? ValidCardNumber,
                12,
                expiryYear,
                "123",
                "player@zeldaarena.test",
                idempotencyKey),
            CancellationToken.None);

    public Task<Result> ConfirmAsync(string code, Guid? paymentId = null) =>
        new ConfirmPaymentCommandHandler(
            CurrentUser(),
            Payments,
            Subscriptions,
            new InMemoryReadRepository<Subscription>(Subscriptions.Entities),
            Plans,
            Orders,
            OrderCancellation(),
            new InMemoryQueryExecutor(),
            Codes,
            Email,
            SignIn,
            UnitOfWork,
            Clock())
        .Handle(
            new ConfirmPaymentCommand(paymentId ?? SinglePayment.Id, code),
            CancellationToken.None);

    public Task<Result> ResendAsync(Guid? paymentId = null) =>
        new ResendPaymentCodeCommandHandler(
            CurrentUser(),
            Payments,
            Codes,
            Email,
            UnitOfWork,
            Clock())
        .Handle(
            new ResendPaymentCodeCommand(paymentId ?? SinglePayment.Id),
            CancellationToken.None);

    public Task<Result> CancelAsync(Guid? paymentId = null) =>
        new CancelPaymentCommandHandler(CurrentUser(), Payments, Subscriptions, Orders, OrderCancellation(), UnitOfWork)
            .Handle(new CancelPaymentCommand(paymentId ?? SinglePayment.Id), CancellationToken.None);

    public Task<PaymentStateDto?> StateAsync(Guid? paymentId = null) =>
        new GetPaymentStateQueryHandler(
            CurrentUser(),
            new InMemoryReadRepository<Payment>(Payments.Entities),
            new InMemoryReadRepository<Order>(Orders.Entities),
            new InMemoryQueryExecutor(),
            Clock())
        .Handle(
            new GetPaymentStateQuery(paymentId ?? SinglePayment.Id),
            CancellationToken.None);

    /// <summary>Полный успешный проход до активной подписки — исходная точка многих проверок.</summary>
    public async Task<Subscription> PayAsync(Guid? planId = null, string idempotencyKey = "form-1")
    {
        var started = await StartAsync(planId, idempotencyKey: idempotencyKey);
        started.IsSuccess.ShouldBeTrue();

        var confirmed = await ConfirmAsync(SentCode, started.Value.PaymentId);
        confirmed.IsSuccess.ShouldBeTrue();

        Email.Forget();

        return Subscriptions.Entities.Single(subscription => subscription.IsActiveAt(_now));
    }

    private StubCurrentUserService CurrentUser() =>
        new() { UserId = SignedInUserId ?? UserId, UserName = "player" };

    private PaymentInitiator Initiator() =>
        new(
            Payments,
            new InMemoryReadRepository<Payment>(Payments.Entities),
            new InMemoryQueryExecutor(),
            Gateway,
            Codes,
            Email,
            Clock());

    private OrderCancellation OrderCancellation() =>
        new(
            Products,
            Payments,
            new InMemoryReadRepository<Payment>(Payments.Entities),
            new CartLocator(
                CurrentUser(),
                new StubGuestCartIdentity(),
                Carts,
                new InMemoryReadRepository<Cart>(Carts.Entities),
                new InMemoryQueryExecutor()),
            new InMemoryQueryExecutor(),
            Clock());

    private FixedDateTimeProvider Clock() => new(_now);
}