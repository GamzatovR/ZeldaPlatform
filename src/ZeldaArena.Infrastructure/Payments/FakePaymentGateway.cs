using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Common.Validation;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Infrastructure.Payments;

/// <summary>
/// Мнимый платёжный провайдер (docs/SPEC.md §7.6). Денег он не списывает: платёж
/// подтверждается кодом из письма, а «авторизация» сводится к проверке номера
/// по алгоритму Луна и определению платёжной системы по BIN.
///
/// Главное здесь — граница. Полный номер карты и CVV существуют только внутри
/// этого вызова: наружу уходит <see cref="CardAuthorization"/> с брендом и последними
/// четырьмя цифрами, и именно они попадают в <c>Payments</c>. Ни в базу, ни в логи,
/// ни в аудит, ни в ответ клиенту номер и CVV не выходят (§7.6, §20 пункт 6).
///
/// Настоящий приём платежей потребовал бы PCI DSS и токенизации на стороне провайдера.
/// Порт <c>IPaymentGateway</c> для того и заведён: заменяется реализация, ядро не
/// трогается (EP-6, docs/adr/ADR-0007).
/// </summary>
public sealed class FakePaymentGateway : IPaymentGateway
{
    public const string ProviderKey = "fake";

    /// <summary>Карта неизвестной системы к оплате не принимается.</summary>
    public const string UnknownBrand = "Unknown";

    public string Key => ProviderKey;

    public Task<Result<CardAuthorization>> AuthorizeAsync(
        CardPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var digits = CardNumber.OnlyDigits(request.CardNumber);

        // Форму уже проверил валидатор; здесь проверка повторяется, потому что порт
        // могут вызвать и не из него. Причину отказа наружу не раскрываем — что именно
        // не так с картой, знает валидатор, а не отказ авторизации.
        if (!IsPlausibleCardNumber(digits) || !IsNotExpired(request))
        {
            return Task.FromResult(Result.Failure<CardAuthorization>(BillingErrors.CardDeclined));
        }

        var authorization = new CardAuthorization(BrandOf(digits), digits[^4..]);

        return Task.FromResult(Result.Success(authorization));
    }

    /// <summary>Платёжная система по первой цифре BIN, как описано в §7.6.</summary>
    public static string BrandOf(string digits)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(digits);

        return digits[0] switch
        {
            '4' => "Visa",
            '5' => "Mastercard",
            '2' => "MIR",
            _ => UnknownBrand,
        };
    }

    private static bool IsPlausibleCardNumber(string digits) =>
        CardNumber.PassesLuhn(digits) && BrandOf(digits) != UnknownBrand;

    private static bool IsNotExpired(CardPaymentRequest request)
    {
        // Диапазон года проверяется до построения даты: конструктор DateOnly
        // бросил бы исключение, а негодные реквизиты — это отказ, а не сбой.
        if (request.ExpiryMonth is < 1 or > 12 || request.ExpiryYear is < 2000 or > 2100)
        {
            return false;
        }

        // Карта действительна до последнего дня указанного месяца включительно.
        // Момент берётся у системных часов, а не у IDateTimeProvider: это внешний
        // провайдер, и он сверяется со своим временем, а не с временем приложения.
        var expiresAfter = new DateOnly(request.ExpiryYear, request.ExpiryMonth, 1).AddMonths(1);

        return expiresAfter > DateOnly.FromDateTime(DateTime.UtcNow);
    }
}