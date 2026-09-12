using System.Globalization;
using System.Security.Cryptography;

using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Orders;

public sealed class OrderNumberGenerator(IReadRepository<Order> orders, IQueryExecutor queryExecutor)
{
    public const string Prefix = "ZA";

    /// <summary>Шесть цифр серийной части — ровно столько печатает <see cref="Format"/>.</summary>
    private const int Range = 1_000_000;
    private const int MaxAttempts = 5;

    public async Task<string> NextAsync(DateTimeOffset moment, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var number = Format(moment.Year, RandomNumberGenerator.GetInt32(Range));

            var taken = await queryExecutor
                .AnyAsync(orders.Query().Where(order => order.Number == number), cancellationToken)
                .ConfigureAwait(false);

            if (!taken)
            {
                return number;
            }
        }

        // Миллион номеров в год при пяти промахах подряд — это не невезение, а сбой.
        throw new InvalidOperationException("Не удалось подобрать свободный номер заказа.");
    }

    public static string Format(int year, int serial) =>
        string.Create(CultureInfo.InvariantCulture, $"{Prefix}-{year:D4}-{serial:D6}");
}