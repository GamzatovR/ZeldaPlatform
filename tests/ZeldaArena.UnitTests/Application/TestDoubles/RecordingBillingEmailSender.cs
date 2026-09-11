using ZeldaArena.Application.Common.Interfaces;

namespace ZeldaArena.UnitTests.Application.TestDoubles;

/// <summary>
/// Запоминает письма биллинга вместо отправки. Код подтверждения сохраняется
/// намеренно: только так проверяется, что пользователю ушёл тот самый код,
/// хеш которого лёг в платёж.
/// </summary>
internal sealed class RecordingBillingEmailSender : IBillingEmailSender
{
    public enum LetterKind
    {
        PaymentCode,
        Receipt,
        OrderReceipt,
        SubscriptionExpired,
    }

    private readonly List<Letter> _letters = [];

    public IReadOnlyList<Letter> Letters => _letters;

    public Letter Single(LetterKind kind) => _letters.Single(letter => letter.Kind == kind);

    public int Count(LetterKind kind) => _letters.Count(letter => letter.Kind == kind);

    /// <summary>Забыть отправленное — чтобы проверять письма одного шага, а не всех подряд.</summary>
    public void Forget() => _letters.Clear();

    public Task SendPaymentCodeAsync(
        string email,
        string? displayName,
        string code,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default)
    {
        _letters.Add(new Letter(LetterKind.PaymentCode, email, code, null, expiresAt));

        return Task.CompletedTask;
    }

    public Task SendPaymentReceiptAsync(
        string email,
        string? displayName,
        string planName,
        decimal amount,
        string currency,
        DateTimeOffset subscriptionEndsAt,
        CancellationToken cancellationToken = default)
    {
        _letters.Add(new Letter(LetterKind.Receipt, email, null, planName, subscriptionEndsAt));

        return Task.CompletedTask;
    }

    public Task SendOrderReceiptAsync(
        string email,
        string? displayName,
        string orderNumber,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default)
    {
        _letters.Add(new Letter(LetterKind.OrderReceipt, email, null, orderNumber, null) { Amount = amount });

        return Task.CompletedTask;
    }

    public Task SendSubscriptionExpiredAsync(
        string email,
        string? displayName,
        string planName,
        CancellationToken cancellationToken = default)
    {
        _letters.Add(new Letter(LetterKind.SubscriptionExpired, email, null, planName, null));

        return Task.CompletedTask;
    }

    /// <summary>Письмо. <c>PlanName</c> — тариф в письмах подписки, номер заказа — в чеке заказа.</summary>
    internal sealed record Letter(
        LetterKind Kind,
        string To,
        string? Code,
        string? PlanName,
        DateTimeOffset? Moment)
    {
        public decimal? Amount { get; init; }
    }
}