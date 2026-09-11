namespace ZeldaArena.Application.Features.Carts;

/// <summary>
/// Чья корзина: вошедшего пользователя или гостя. Ровно одно из двух — так же, как
/// у самой <c>Cart</c>.
/// </summary>
public sealed record CartOwner
{
    private CartOwner(Guid? userId, Guid? anonymousId)
    {
        UserId = userId;
        AnonymousId = anonymousId;
    }

    public Guid? UserId { get; }

    public Guid? AnonymousId { get; }

    public static CartOwner ForUser(Guid userId) => new(userId, null);

    public static CartOwner ForGuest(Guid anonymousId) => new(null, anonymousId);
}