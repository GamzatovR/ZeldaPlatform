namespace ZeldaArena.Application.Features.Payments;

public static class PaymentIdempotency
{
    public const int MaxClientKeyLength = 40;

    private const char Separator = ':';

    public static string KeyFor(Guid userId, string clientKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientKey);

        return $"{userId:N}{Separator}{clientKey.Trim()}";
    }
}