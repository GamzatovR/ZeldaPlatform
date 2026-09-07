using ZeldaArena.Domain.Common;
using ZeldaArena.Domain.Common.Exceptions;

namespace ZeldaArena.Domain.ValueObjects;

/// <summary>
/// Адрес доставки. Хранится снапшотом внутри заказа (шесть столбцов таблицы Orders),
/// а не ссылкой на справочник: адресной книги в проекте нет, и заказ не должен
/// меняться задним числом вслед за профилем пользователя (docs/SPEC.md §6).
/// </summary>
public sealed class ShippingAddress : ValueObject
{
    /// <summary>Конструктор для материализации из БД, см. <see cref="Money"/>.</summary>
    private ShippingAddress() =>
        Recipient = Phone = Country = City = Street = PostalCode = string.Empty;

    public ShippingAddress(
        string recipient,
        string phone,
        string country,
        string city,
        string street,
        string postalCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient);
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);
        ArgumentException.ThrowIfNullOrWhiteSpace(country);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(street);
        ArgumentException.ThrowIfNullOrWhiteSpace(postalCode);

        InvariantViolationException.ThrowIf(
            phone.Trim().Length < 5,
            "address.invalid_phone",
            "Телефон получателя слишком короткий.");

        Recipient = recipient.Trim();
        Phone = phone.Trim();
        Country = country.Trim();
        City = city.Trim();
        Street = street.Trim();
        PostalCode = postalCode.Trim();
    }

    public string Recipient { get; }

    public string Phone { get; }

    public string Country { get; }

    public string City { get; }

    public string Street { get; }

    public string PostalCode { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Recipient;
        yield return Phone;
        yield return Country;
        yield return City;
        yield return Street;
        yield return PostalCode;
    }
}