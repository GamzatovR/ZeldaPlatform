namespace ZeldaArena.Application.Features.Carts;

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