using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Application.Common.Models;
using ZeldaArena.Application.Common.Models.Billing;
using ZeldaArena.Application.Common.Validation;
using ZeldaArena.Domain.Common;

namespace ZeldaArena.Infrastructure.Payments;

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

        if (!IsPlausibleCardNumber(digits) || !IsNotExpired(request))
        {
            return Task.FromResult(Result.Failure<CardAuthorization>(BillingErrors.CardDeclined));
        }

        var authorization = new CardAuthorization(BrandOf(digits), digits[^4..]);

        return Task.FromResult(Result.Success(authorization));
    }

    /// <summary>Платёжная система по первой цифре BIN, как описано в</summary>
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
        var expiresAfter = new DateOnly(request.ExpiryYear, request.ExpiryMonth, 1).AddMonths(1);

        return expiresAfter > DateOnly.FromDateTime(DateTime.UtcNow);
    }
}