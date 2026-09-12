using ZeldaArena.Application.Common.Interfaces;
using ZeldaArena.Domain.Shop;

namespace ZeldaArena.Application.Features.Carts;

public sealed class CartLocator(
    ICurrentUserService currentUser,
    IGuestCartIdentity guest,
    IRepository<Cart> carts,
    IReadRepository<Cart> cartsForRead,
    IQueryExecutor queryExecutor)
{
    public CartOwner? CurrentOwner() =>
        currentUser.UserId is { } userId ? CartOwner.ForUser(userId)
        : guest.AnonymousId is { } anonymousId ? CartOwner.ForGuest(anonymousId)
        : null;

    public IQueryable<Cart> OwnedBy(CartOwner owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return owner.UserId is { } userId
            ? cartsForRead.Query().Where(cart => cart.UserId == userId)
            : cartsForRead.Query().Where(cart => cart.AnonymousId == owner.AnonymousId && cart.UserId == null);
    }

    public Task<Guid?> FindIdAsync(CartOwner owner, CancellationToken cancellationToken) =>
        queryExecutor.FirstOrDefaultAsync(OwnedBy(owner).Select(cart => (Guid?)cart.Id), cancellationToken);

    /// <summary>Отслеживаемая корзина вместе с позициями или <c>null</c>, если её ещё нет.</summary>
    public async Task<Cart?> FindAsync(CartOwner owner, CancellationToken cancellationToken)
    {
        var id = await FindIdAsync(owner, cancellationToken).ConfigureAwait(false);

        return id is null ? null : await carts.GetByIdAsync(id.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Cart> GetOrCreateAsync(CartOwner owner, CancellationToken cancellationToken)
    {
        var existing = await FindAsync(owner, cancellationToken).ConfigureAwait(false);

        if (existing is not null)
        {
            return existing;
        }

        var created = owner.UserId is { } userId
            ? Cart.ForUser(userId)
            : Cart.ForGuest(owner.AnonymousId!.Value);

        await carts.AddAsync(created, cancellationToken).ConfigureAwait(false);

        return created;
    }
}