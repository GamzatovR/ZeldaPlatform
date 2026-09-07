using System.Security.Cryptography;
using System.Text;

using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;
using ZeldaArena.Domain.Enums;
using ZeldaArena.Domain.Events;
using ZeldaArena.Domain.ValueObjects;

namespace ZeldaArena.Domain.Billing;

/// <summary>
/// Платёж — общий механизм для подписок и заказов магазина (docs/SPEC.md §7.6).
///
/// Полного номера карты и CVV в этой сущности нет и не появится: хранятся только
/// последние четыре цифры и платёжная система. Код подтверждения хранится хешем;
/// исходный код известен только письму, ушедшему пользователю.
/// </summary>
public class Payment : BaseEntity, IAuditableEntity
{
    public const int MaxAttempts = 5;

    private Payment()
    {
    }

    public Guid UserId { get; private set; }

    public PaymentPurpose Purpose { get; private set; }

    public Guid? SubscriptionId { get; private set; }

    public Guid? OrderId { get; private set; }

    public Money Amount { get; private set; } = null!;

    public PaymentStatus Status { get; private set; }

    /// <summary>Последние четыре цифры карты — единственное, что известно о её номере.</summary>
    public string CardLast4 { get; private set; } = null!;

    /// <summary>Платёжная система, определённая по BIN: Visa, Mastercard, MIR.</summary>
    public string CardBrand { get; private set; } = null!;

    public string ConfirmationEmail { get; private set; } = null!;

    /// <summary>Хеш шестизначного кода. Сам код в базе не хранится.</summary>
    public string ConfirmationCodeHash { get; private set; } = null!;

    public DateTimeOffset ConfirmationExpiresAt { get; private set; }

    public int ConfirmationAttemptsLeft { get; private set; }

    /// <summary>Ключ идемпотентности: повторная отправка формы не создаёт второй платёж.</summary>
    public string IdempotencyKey { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public DateTimeOffset? PaidAt { get; private set; }

    public string? FailureReason { get; private set; }

    public bool IsPending => Status == PaymentStatus.Pending;

    public static Payment Start(
        Guid userId,
        PaymentPurpose purpose,
        Money amount,
        string cardLast4,
        string cardBrand,
        string confirmationEmail,
        string confirmationCodeHash,
        DateTimeOffset confirmationExpiresAt,
        string idempotencyKey,
        Guid? subscriptionId = null,
        Guid? orderId = null)
    {
        ArgumentNullException.ThrowIfNull(amount);
        ArgumentException.ThrowIfNullOrWhiteSpace(cardBrand);
        ArgumentException.ThrowIfNullOrWhiteSpace(confirmationEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(confirmationCodeHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);

        InvariantViolationException.ThrowIf(
            userId == Guid.Empty,
            "payment.user_required",
            "У платежа обязан быть плательщик.");

        InvariantViolationException.ThrowIf(
            amount.IsZero,
            "payment.zero_amount",
            "Платёж на нулевую сумму не имеет смысла.");

        InvariantViolationException.ThrowIf(
            purpose == PaymentPurpose.Subscription && orderId is not null,
            "payment.purpose_mismatch",
            "Платёж за подписку не может ссылаться на заказ.");

        InvariantViolationException.ThrowIf(
            purpose == PaymentPurpose.Order && subscriptionId is not null,
            "payment.purpose_mismatch",
            "Платёж за заказ не может ссылаться на подписку.");

        return new Payment
        {
            UserId = userId,
            Purpose = purpose,
            SubscriptionId = subscriptionId,
            OrderId = orderId,
            Amount = amount,
            Status = PaymentStatus.Pending,
            CardLast4 = RequireCardLast4(cardLast4),
            CardBrand = cardBrand.Trim(),
            ConfirmationEmail = confirmationEmail.Trim(),
            ConfirmationCodeHash = confirmationCodeHash,
            ConfirmationExpiresAt = confirmationExpiresAt,
            ConfirmationAttemptsLeft = MaxAttempts,
            IdempotencyKey = idempotencyKey.Trim(),
        };
    }

    /// <summary>
    /// Сверяет хеш введённого кода. Сравнение постоянного времени, чтобы по задержке
    /// ответа нельзя было подбирать код посимвольно. Неверная попытка уменьшает счётчик;
    /// когда попытки кончились или срок истёк, платёж переходит в Failed (docs/SPEC.md §7.6).
    /// </summary>
    public PaymentConfirmationResult Confirm(string providedCodeHash, DateTimeOffset moment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providedCodeHash);

        if (Status != PaymentStatus.Pending)
        {
            return PaymentConfirmationResult.AlreadyProcessed;
        }

        if (moment > ConfirmationExpiresAt)
        {
            Fail("payment.code_expired");
            return PaymentConfirmationResult.Expired;
        }

        if (ConfirmationAttemptsLeft < 1)
        {
            Fail("payment.no_attempts_left");
            return PaymentConfirmationResult.NoAttemptsLeft;
        }

        if (!HashesMatch(ConfirmationCodeHash, providedCodeHash))
        {
            ConfirmationAttemptsLeft--;

            if (ConfirmationAttemptsLeft < 1)
            {
                Fail("payment.no_attempts_left");
                return PaymentConfirmationResult.NoAttemptsLeft;
            }

            return PaymentConfirmationResult.WrongCode;
        }

        Status = PaymentStatus.Succeeded;
        PaidAt = moment;
        FailureReason = null;

        Raise(new PaymentConfirmedEvent(
            Id,
            UserId,
            Purpose,
            SubscriptionId,
            OrderId,
            Amount.Amount,
            Amount.Currency));

        return PaymentConfirmationResult.Succeeded;
    }

    /// <summary>
    /// Новый код взамен истёкшего: срок и счётчик попыток начинаются заново.
    /// Частоту повторной отправки ограничивает rate limiting на эндпоинте, а не сущность.
    /// </summary>
    public void ReissueCode(string confirmationCodeHash, DateTimeOffset expiresAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(confirmationCodeHash);

        InvariantViolationException.ThrowIf(
            !IsPending,
            "payment.not_pending",
            $"Код высылается только по платежу в ожидании, текущий статус — {Status}.");

        ConfirmationCodeHash = confirmationCodeHash;
        ConfirmationExpiresAt = expiresAt;
        ConfirmationAttemptsLeft = MaxAttempts;
    }

    public void AttachSubscription(Guid subscriptionId)
    {
        InvariantViolationException.ThrowIf(
            Purpose != PaymentPurpose.Subscription,
            "payment.purpose_mismatch",
            "Привязать подписку можно только к платежу за подписку.");

        SubscriptionId = subscriptionId;
    }

    public void AttachOrder(Guid orderId)
    {
        InvariantViolationException.ThrowIf(
            Purpose != PaymentPurpose.Order,
            "payment.purpose_mismatch",
            "Привязать заказ можно только к платежу за заказ.");

        OrderId = orderId;
    }

    public void Fail(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        InvariantViolationException.ThrowIf(
            Status == PaymentStatus.Succeeded,
            "payment.already_succeeded",
            "Успешный платёж нельзя объявить неудачным.");

        Status = PaymentStatus.Failed;
        FailureReason = reason;
    }

    public void CancelByUser()
    {
        InvariantViolationException.ThrowIf(
            !IsPending,
            "payment.not_pending",
            $"Отменить можно только платёж в ожидании, текущий статус — {Status}.");

        Status = PaymentStatus.Canceled;
    }

    private static bool HashesMatch(string expected, string provided)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var providedBytes = Encoding.UTF8.GetBytes(provided);

        return CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }

    private static string RequireCardLast4(string cardLast4)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cardLast4);

        var normalized = cardLast4.Trim();

        InvariantViolationException.ThrowIf(
            normalized.Length != 4 || !normalized.All(char.IsAsciiDigit),
            "payment.invalid_card_last4",
            "Хранятся ровно четыре последние цифры номера карты.");

        return normalized;
    }
}